# ZeroMQ Protocol definition
The bot supports changing configuration via commands and is capable of publishing updates.

# TODO
##### Documentation
- Add example input
- Add example output

##### Code
- Everything

##### Other
- Should bot stop fetching data when in stopped state?

# Commands
Commands use the [request-reply](http://zguide.zeromq.org/page:all#The-Simple-Reply-Envelope) pattern in ZeroMQ. A program may request a certain action of the bot, and the bot may respond in three different ways:
- `success`: The requested action has been processed succesfully.
- `failure`: The action is invalid or prohibited in the current context.
- `error`: An error has occurred. The bot may have crashed or the request was invalid.

Failure and error states will contain a message indicating what went wrong. Three different types of commands are listed. Status commands request aspects of the current configuration of the bot. Active commands may be executed while the bot is in a `started` or `stopped` state. Passive commands may __only__ be executed while the bot is in a `stopped` state. 

The bot has two states:
- `Started`: The bot is actively running
- `Stopped`: The bot is alive, but is not trading or fetching data (?)

---

## Active commands
Simple commands may be executed while the bot is in a `started` state.

##### Command: Stop bot
Command: `command_stop_bot`

Arguments: None

Failure states:
- Failure will be returned if the bot is already in a `stopped` state

---

##### Command: Reset holding time
Command: `command_reset_holding_time`

Arguments: None

Failure states:
- Failure will be returned if the bot is not in a holding position

---

##### Command: Switch to different currency
Command: `command_switch_currency`

Arguments:

| Argument    | Name                 | Description          | Example |
|-------------|----------------------|----------------------|---------|
| Currency    | `arg_currency`       | The currency to switch to| btc |

Failure states:
- Failure will be returned if the bot is already in that currency
- Failure will be returned if the Binance account does not have enough BNB to the fees
- Failure will be returned if the bot does not recognise the currency

---

##### Command: Revert to base currency
Command: `command_revert_basecurrency`

Arguments: None

Failure states:
- Failure will be returned if the bot is already in that currency
- Failure will be returned if the Binance account does not have enough BNB to the fees

---

##### Command: Add trading pair
Command: `command_add_tradingpair`

Note: This command may be run in any state.

Arguments:

| Argument    | Name                 | Description          | Example |
|-------------|----------------------|----------------------|---------|
| Trading pair| `arg_tradingpair`    | The trading pair     | bnbbtc  |

Failure states:
- Failure will be returned if the tradingpair is already included in the list
- Failure will be returned if the tradingpair is not recognised

---

##### Command: Remove trading pair
Command: `command_remove_tradingpair`

Note: This command may be run in any state.

Arguments:

| Argument    | Name                 | Description          | Example |
|-------------|----------------------|----------------------|---------|
| Trading pair| `arg_tradingpair`    | The trading pair     | bnbbtc  |

Failure states:
- Failure will be returned if the tradingpair is not in the list of current tradingpairs
- Failure will be returned if the tradingpair is not recognised

---

## Passive commands
Simple commands may be executed while the bot is in a `stopped` state.

##### Command: Start bot
Command: `command_start_bot`

Arguments: None

Failure states:
- Failure will be returned if the bot is already in a `started` state

---

##### Command: Change configuration: base currency
Command: `command_change_basecurrency`

Arguments:

| Argument    | Name                 | Description          | Example |
|-------------|----------------------|----------------------|---------|
| Currency    | `arg_currency`       | The new base currency| btc     |

Failure states:
- Failure will be returned if the currency is not one of btc, eth, usdt

---

##### Command: Change configuration: check time
Command: `command_change_checktime`

Arguments:

| Argument    | Name                 | Description          | Example |
|-------------|----------------------|----------------------|---------|
| Check time  | `arg_time`           | Hours to look back at different currencies | 18     |

Failure states:
- Failure will be returned if the time is not between 1 and 72

---

##### Command: Change configuration: hold time
Command: `command_change_holdtime`

Arguments:

| Argument    | Name                 | Description          | Example |
|-------------|----------------------|----------------------|---------|
| Hold time   | `arg_time`           | Hours to hold a position | 12  |

Failure states:
- Failure will be returned if the time is not between 1 and 72

---

## Status updates
These commands may be used to gather information about the current configuration of the bot. These commands have no arguments and no failure states. They can be run in any context and in all states of the bot.

##### Command: Get current trading pairs
Command: `command_get_tradingpairs`

Example output: `[bnbbtc, ethbtc]`

---

##### Command: Get current base currency
Command: `command_get_basecurrency`

Example output: `btc`

---

##### Command: Get current checking time
Command: `command_get_checktime`

Example output: `18`

---

##### Command: Get current holding time
Command: `command_get_holdtime`

Example output: `12`

---
---

# Broadcasts
Broadcasts use the [pub-sub](http://zguide.zeromq.org/page:all#Chapter-Advanced-Pub-Sub-Patterns) pattern in ZeroMQ. A program may subscribe to a topic of the bot, and the bot publishes updates at a regular interval or on change on these topics. When subscribing to a topic, a cached value is sent.

##### Broadcast: Status
Topic: `topic_status`

Publish-mode: On change

Example output: `started` or `stopped`

---

##### Broadcast: Remaining holding time
Topic: `topic_holdtime`

Publish-mode: Every minute

Example output: `23400` (seconds)
