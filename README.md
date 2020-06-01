# DnevnikRuApi
Библиотека для работы с API Dnevnik.ru с токеном или без него

Большая благодарность [kesha1225](https://github.com/kesha1225) он помог разобраться с тем как получить токен.

## Инициализация

```C#
ApiDiary api = new ApiDiary(string keyAccess);
```
или
```C#
ApiDiary api = new ApiDiary(string login, string pass);
```
или
```C#
ApiDiary api = new ApiDiary(string pass, string login, string client_id, string client_secret, string scope);
```

Позже будет написан мануал по всем методам и функциям!
