# FileTransformer

Simple file Extract-Transform-Load (ETL) tool.

Usage:
```ps
FileTransformer -r $readDirectory -w $writeDirectory
```

Description
- Registers a small DI pipeline to read, validate, transform, and write files.
- File handlers are extendable using the Chain of Responsibility design pattern.
- Handlers are executed in registration order (e.g., validation before reading).

Registered services (summary)
- FolderOptions — optional, e.g. ReadDirectory, 'ToTransform' by default.
- IFileSystem -> FileSystem (singleton)
- IFileReader -> FileReader (scoped)
- IFileWriter -> FileWriter (scoped)
- IFolderHandler -> FolderHandler (scoped)
- IExecuteService -> FolderHandlerService (scoped)
- IFileHandler -> FileHandlerFactory (scoped)
- IExecuteNextHandler implementations (scoped) — registered in order:
  1. JsonValidationHandler
  2. JsonFileReaderHandler

How it runs (high level)
- FolderHandlerService enumerates files with FolderHandler and triggers file processing.
- Each file is sent through the IExecuteNextHandler pipeline (validation → read → transform → write).
- Add more handlers by registering additional IExecuteNextHandler implementations in the desired order.

Extending
- Implement IExecuteNextHandler and register it:
```cs
services.AddScoped<IExecuteNextHandler, YourCustomHandler>();
```
