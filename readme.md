
# ClickLogger

A mouse event logger with screenshot capture, designed as a modern alternative to Microsoft's deprecated Problem Steps Recorder (PSR).

## Features

- **Mouse Event Logging**: Records click events in real-time
- **Event Details**: Saves timestamp, event name and parameter in a csv file
- **User Session Tracking**: Organizes csv logs by start time of the session

## Getting Started


### Prerequisites

- Windows 10 or later
- .NET 8 or later


### Installation

1. Clone the repository:
    ```
    git clone https://github.com/BernhardSchmitt/ClickLogger.git
    ```

2. Build the project:
    ```
    dotnet build
    ```

3. Run the application:
    ```
    dotnet run
    ```

4. Create release build:
    ```
    dotnet publish ClickLogger.csproj -c Release
    ```

## Usage

1. Launch ClickLogger
2. Click **REC** to begin recording mouse events
3. Perform the steps you want to document
4. Click **STOP** when finished
5. Click **Open Log Folder** to check the csv file of your session

## Configuration

### Blacklisting
Place a blacklist.csv file next to the executable to blacklist applications by process name and optional window name.  
```
ProcessName,WindowName
ClickLogger
dotnet,Click Logger
```

## License

GNU General Public License v3.0 - see gpl-3.0.md file for details
