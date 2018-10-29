## Who keeps track of the trades?
The TradingProvider logs all trades to the database

## Who keeps track of state switches?
The StateManager logs all switches to the database.

## When is a backtest finished?
When the BacktestTimerProvider senses that the time it keeps track of, exceeds the scope of the backtest.

## How is the backtest stopped?
The BacktestTimerProvider will refuse to execute given callback if the time has exceeded the scope.

## When is the the report created
The BacktestTimerProvider will, upon recognising that the time has exceeded the scope, trigger the BacktestReportGenerator.

## How does the backtest logger operate?
The BacktestReportGenerator will query the database for all useful info and nuke the table afterwards, since the database should not be bloated with rudimentary backtest results.
