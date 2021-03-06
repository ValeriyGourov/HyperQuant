# HyperQuant
Тестовое задание для "HyperQuant"

## 1. Терминология
### Common

**Currency (Валюта)** — валюта, тип денег. Например, доллар США или биткоин.

**Currency pair (Валютная пара)** — пара валют, которая означает отношение одной валюты к другой. Как и в простом математическом отношении порядок имеет значение. Например, доллар США к биткоину.

**Symbol (Символ)** — символьное обозначение валюты или валютной пары. Например, USD или USDBTC.

**Platform (Платформа)** — система(, которая является основой для разной деятельности). Программная платформа — это программная система, обычно со своим API.

**Exchange (Биржа)** — учреждение для заключения финансовых и коммерческих сделок. Часто биржа реализуется как программная платформа, доступная через интернет в любой точке мира.
### Development

**Connector (Коннектор)** — в широком смысле это класс или группа классов, через которые мы подключаемся к API отдельных платформ, чтобы выполнить какое-либо действие. В нашем случае такие коннекторы разбиваются на клиенты, которые приводят REST и WS-интерфейсы отдельных платформ к единому интерфейсу на уровне кода, то есть реализованному в классе, а также на коннекторы в узком смысле, которые используют клиенты и реализуют более сложную логику использования API платформ, которая включает реконнекты, обработку ошибок, фиксирование скачанных и пропущенных данных, которые нужно скачать позже и так далее.

**Client (Клиент)** — класс, через который мы в программе обращаемся к другим внешним API: REST и WS API бирж, например. Другими словами, он преобразует внешний общий API в API на уровне классов конкретного языка программирования. Обычно, через одинаковый такой программный интерфейс можно обращаться к разным платформам, собственные API которых отличаются.

## 2. Тестовое задание

Необходимо реализовать коннектор под исходный интерфейс(пункт 3) на C# (Class Library, желательно на .Core ), а так-же покрыть его юнит-тестами на xUnit/Unit Test Project (в зависимости от frameworka),  или сделать в отдельном проекте простой вывод на GUI Framework (WPF / UWP) на основе паттерна MVVM, не WinForms. 
Что должно быть в этом коннекторе:

* Класс клиента для REST API  биржи Bitfinex, который реализует 2 функции:
  * Получение трейдов (trades) 
  * Получение свечей (candles) 
* Класс клиента для Websocket API  биржи Bitfinex, который реализует 2 функции:
  * Получение трейдов (trades)
  * Получение свечей (candles)

API Bitfinex ([ссылка на API](https://docs.bitfinex.com/v2/docs)) . Использовать версию API v2,
## 3. Интерфейс ITestConnector

Интерфейс, модели Trade и Candle доступны по [ссылке](https://drive.google.com/open?id=1anniFh4qyAoFjoCu6IK3LSLFpyOc9wtP). Это упрощенный вариант интерфейса, если вам не будет хватать входных параметров, можно их дополнить но с объяснением, почему Вы так сделали.
Так-же, если посчитаете некоторые параметры лишними, можете их убрать, но опять же с объяснением (Можно в комментариях кода)
