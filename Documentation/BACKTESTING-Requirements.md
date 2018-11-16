# Backtesting documentation and requirements.

## Synopsis
Algorithms have to be tested by a system that mimics the interface 
of exchange- and other provider. This mitigates the need for the risky and error prone
process of porting algorithm implementations between two environments. Secondly, it ensures
a fast development cycle.
 
Additionally, the output of a backtesting run is
constraint a format that allows for fast and effective evaluation
of algorithms on both an individual level as well as relative to one another.
The result from a trade should be calculated in every Sell trade, by dividing the sell price over the buy price, so 0.15(sell)/0.1(buy) - 0.0015(fees for two trades) = 1.4985 , meaning a 49.85% return on that particular trade



## Adjustable parameters
On top of the algorithm parameters, the following should be configurable.

* Fees per transaction
* Currency used to pay fees
    * Methodology for paying fees (e.g. 3rd asset like BNB or substracted from trade directly.)
* Starting- and end date of data input
* Trading pairs to evaluate
    * Optional varying starting and end dates per pair
    
## Event Output Format

### Executed trade
| timestamp             | base   | asset | BUY/SELL | quantity | price   | total assets after(url?)| result minus fees |
| ---                   | ---    | ---   | ---      | ---      | ---     | ---                     | ---               |
| 2018-11-09 : 16:42    | ETH    | VET   | SELL     | 2452.23  | 0.00075 | /bk/assets/4244.md      | 1.0.135           |

### State Switch
| timestamp             | state from              | state to                |
| ---                   | ---                     | ---                     |
| 2018-11-09 : 16:42    | CheckPositionValidity   | WaitHoldingTime         |
| 2018-11-09 : 16:58    | WaitHoldingTime         | CheckPositionValidity   |
| 2018-11-09 : 16:42    | CheckPositionValidity   | BuyState                |

### Final report appendix

* Total number of trades
* Starting state of assets
* End state of assets
* Total value gained/lost in base currency
* Total volume moved in base currency
* Percentage of losing and winning trades
* Highest amount of consecutive losses and wins
* Average size and standard deviation of winning and losing trades

Statistics about whether trades are profitible are based upon the portfolio value since the last filled trade
