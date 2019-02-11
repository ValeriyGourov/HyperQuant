using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bitfinex.Connector.Models.WebSockeSubscription
{
    internal abstract class SubscriptionBase : IDisposable
    {
        private readonly Uri _serverUri = new Uri("wss://api-pub.bitfinex.com/ws/2");
        private const int _receiveChunkSize = 1024;
        private readonly ClientWebSocket _socket = new ClientWebSocket();
        protected Dictionary<string, object> _additionalInitialMessageItems;
        private long _channelId;
        private readonly JsonSerializer _jsonSerializer = new JsonSerializer();

        protected abstract string Channel { get; }

        public async Task ConnectAsync()
        {
            var initialMessage = new Dictionary<string, object>(_additionalInitialMessageItems.AsEnumerable())
            {
                { "event", RequestEvent.Subscribe },
                { "channel", Channel }
            };

            await _socket.ConnectAsync(_serverUri, CancellationToken.None).ConfigureAwait(false);
            await SendMessagesAsync(initialMessage).ConfigureAwait(false);
            await StartListen().ConfigureAwait(false);
        }

        private async Task<bool> SendMessagesAsync(object message)
        {
            if (_socket.State != WebSocketState.Open)
            {
                throw new Exception("Соединение не открыто.");
            }

            string json = JsonConvert.SerializeObject(message);
            ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Helper.GetBytes(json));
            await _socket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None)
                .ConfigureAwait(false);

            return _socket.State == WebSocketState.Open;
        }

        private async Task StartListen()
        {
            byte[] buffer = new byte[_receiveChunkSize];

            try
            {
                while (_socket.State == WebSocketState.Open)
                {
                    StringBuilder stringBuilder = new StringBuilder(_receiveChunkSize);
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await _socket.ReceiveAsync(buffer, CancellationToken.None)
                            .ConfigureAwait(false);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            // TODO: Вынести закрытие в отдельный метод?
                            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None)
                                .ConfigureAwait(false);
                        }
                        else
                        {
                            stringBuilder.Append(Helper.GetString(buffer, result.Count));
                        }
                    } while (!result.EndOfMessage);

                    string json = stringBuilder.ToString();
                    using (JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json)))
                    {
                        await jsonTextReader.ReadAsync();
                        if (jsonTextReader.TokenType == JsonToken.StartObject)
                        {
                            JObject jObject = JObject.Parse(json);
                            if (jObject.ContainsKey("event"))
                            {
                                ProcessEvent(jObject);
                            }
                        }
                        else if (jsonTextReader.TokenType == JsonToken.StartArray)
                        {
                            await jsonTextReader.ReadAsync();
                            if (jsonTextReader.TokenType == JsonToken.Integer
                                && (long)jsonTextReader.Value == _channelId)
                            {
                                await jsonTextReader.ReadAsync();
                                if (jsonTextReader.TokenType == JsonToken.StartArray)
                                {
                                    await jsonTextReader.ReadAsync();
                                    if (jsonTextReader.TokenType == JsonToken.StartArray)
                                    {
                                        await ProcessThreeLevelData(jsonTextReader);
                                    }
                                    else
                                    {
                                        await ProcessTwoLevelData(jsonTextReader);
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new FormatException("Неизвестный формат данных с сервера.");
                        }
                    }
                }
            }
            catch (IOException)
            {

                throw;
            }
            finally
            {
                // TODO: Закрыть подключение.
            }
        }

        public void Dispose()
        {
            // TODO: Вызвать закрытие подключения.
            _socket.Abort();
            _socket.Dispose();
        }

        protected virtual void ProcessEvent(JObject jObject)
        {
            ResponseEvent @event = jObject["event"].ToObject<ResponseEvent>();
            switch (@event)
            {
                case ResponseEvent.Error:
                    break;
                case ResponseEvent.Info:
                    // TODO: Проверить номер версии. Если изменился - фиксировать предупреждение.
                    // TODO: Проверить статус платформы. Если 0 (обслуживание) - фиксировать предупреждение.

                    const string propertyCode = "code";
                    if (jObject.ContainsKey(propertyCode))
                    {
                        int code = jObject[propertyCode].ToObject<int>();
                        switch (code)
                        {
                            case 20051:
                                // TODO: Stop/Restart Websocket Server (please reconnect)
                                break;
                            case 20060:
                                // TODO: Entering in Maintenance mode. Please pause any activity and resume after receiving the info message 20061 (it should take 120 seconds at most).
                                break;
                            case 20061:
                                // TODO: Maintenance ended. You can resume normal activity. It is advised to unsubscribe/subscribe again all channels.
                                break;
                        }
                    }
                    break;
                case ResponseEvent.Subscribed:
                    const string propertyChanId = "chanId";
                    if (jObject.ContainsKey(propertyChanId))
                    {
                        _channelId = jObject[propertyChanId].ToObject<long>();
                    }
                    else
                    {
                        throw new FormatException($"В теле ответа отсутствует ожидаемое свойство '{propertyChanId}'.");
                    }
                    break;
                case ResponseEvent.Unsubscribed:
                    break;
                default:
                    break;
            }
        }

        protected abstract Task ProcessThreeLevelData(JsonTextReader jsonTextReader);

        protected abstract Task ProcessTwoLevelData(JsonTextReader jsonTextReader);

        protected async Task<List<T>> ReadThreeLevelData<T>(JsonTextReader jsonTextReader, Func<List<string>, T> convertHandler)
        {
            List<T> items = new List<T>();

            while (await jsonTextReader.ReadAsync())
            {
                if (jsonTextReader.TokenType != JsonToken.StartArray
                    && jsonTextReader.TokenType != JsonToken.EndArray)
                {
                    T dataItem = await ReadDataItem<T>(jsonTextReader, convertHandler);
                    items.Add(dataItem);
                }
            };

            return items;
        }

        protected Task<T> ReadTwoLevelData<T>(JsonTextReader jsonTextReader, Func<List<string>, T> convertHandler)
        {
            return ReadDataItem<T>(jsonTextReader, convertHandler);
        }

        private async Task<T> ReadDataItem<T>(JsonTextReader jsonTextReader, Func<List<string>, T> convertHandler)
        {
            var dataItem = new List<string>();
            do
            {
                string value = _jsonSerializer.Deserialize<string>(jsonTextReader);
                dataItem.Add(value);
            } while (!await jsonTextReader.ReadAsync() || jsonTextReader.TokenType != JsonToken.EndArray);

            return convertHandler(dataItem);
        }
    }
}
