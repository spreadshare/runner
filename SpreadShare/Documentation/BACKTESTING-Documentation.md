## Who keeps track of the trades?
The trading provider logs all trades to the database

## Who keeps track of state switches?
The state manager logs all switches to the database.

## When is a backtest finished?
When the backtest timer provider senses that the time it keeps track of exceeds
the scope of the backtest.

## How is the backtest stopped?
The backtest timer provider will refuse to execute given callback if the time has exceeded the scope.

## When is the the report created
The backtest timer provider will, upon recognising that the time has exceeded the scope, fire of the
backtest logger.

## How does the backtest logger operate?
The backtest logger will query the database for all useful info and nuke the table afterwards, since the
database should not be bloated with rudimentary backtest results.
