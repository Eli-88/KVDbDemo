# KeyValueDB Demo

This is a simple C# exercise that demonstrates the use of the unsafe feature. 
It focuses on using pointers and direct memory access to optimize performance.
A skiplist is used in the example to show how unsafe code can work with data structures.

## Usage
The server starts on port 8080 and is ready to accept requests.
Currently it can only run on mac os due to interop for mmap


## Class Diagram
```mermaid
---
title: Simplified Class Diagram
---
classDiagram
    IHandleRequest <|-- HandleAddRequest
    IHandleRequest <|-- HandleGetRequest
    IHandleRequest <|-- HandleRemoveRequest
    IRequestDispatcher <|-- RequestDispatcher
    IStorage <|-- SkipListStorage
    IStorage <|-- ArrayStorage
    IStorage <|-- HashMapStorage
    Service --> IRequestDispatcher 
    Service ..> IStorage
    IRequestDispatcher ..> IStorage
    IHandleRequest ..> IStorage
    IRequestDispatcher *-- IHandleRequest

    class Service {
        +Service(host: string, port: string, dispatcher: IRequestDispatcher)
        +Run(IStorage)
        -ProcessTask(IStorage)
    }

    class IRequestDispatcher {
        +OnDispatch(storage: IStorage, path: string, body: string) string?
    }

    class RequestDispatcher {
        +OnDispatch(storage: IStorage, path: string, body: string) string?
    }

    class IHandleRequest {
        +OnRequest(storage: IStorage, body: string) string
    }

    class HandleAddRequest {
        +OnRequest(storage: IStorage, body: string) string
    }

    class HandleGetRequest {
        +OnRequest(storage: IStorage, body: string) string
    }

    class HandleRemoveRequest {
        +OnRequest(storage: IStorage, body: string) string
    }

    class IStorage {
        +Retrieve(key: int) : (int, bool)
        +Insert(key: int, value: int)
        +Remove(key: int)
    }

    class SkipListStorage {
        +Retrieve(key: int) : (int, bool)
        +Insert(key: int, value: int)
        +Remove(key: int)
    }

    class ArrayStorage {
        +Retrieve(key: int) : (int, bool)
        +Insert(key: int, value: int)
        +Remove(key: int)
    }

    class HashMapStorage {
        +Retrieve(key: int) : (int, bool)
        +Insert(key: int, value: int)
        +Remove(key: int)
    }

```


### Storing a Value

```sh
curl -X POST "http://localhost:8080/add" -H "Content-Type: application/json" -d '{"Key": 1, "Value": 200}'
```

### Retrieving a Value

```sh
curl -X POST "http://localhost:8080/get" -H "Content-Type: application/json" -d '{"Key": 1}'
```

### Deleting a Value

```sh
curl -X POST "http://localhost:8080/remove" -H "Content-Type: application/json" -d '{"Key": 1}'
```