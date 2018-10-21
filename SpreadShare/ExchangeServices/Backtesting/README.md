# Backtesting documentation and requirements.

## Synopsis
Algorithms have to be tested by a system that mimics the interface 
of exchange- and other provider. This mitigates the need for the risky and error prone
process of porting algorithm implementations between two environments. Secondly, it ensures
a fast development cycle.
 
Additionally, the output of a backtesting run is
constraint a format that allows for fast and effective evaluation
of algorithms on both an individual level as well as relative to one another.



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
| timestamp             | base   | asset | BUY/SELL | quantity | total assets before (url?)| total assets after(url?)| fees |
| ---                   | ---    | ---   | ---      | ---      | ---                       | ---                     |  --- |
| 2018-11-09 : 16:42    | ETH    | VET   | BUY      | 2452.23  | /bk/assets/4234.md        | /bk/assets/4244.md      | 0.003 ETH |

### State Switch
| timestamp             | state from              | state to                |
| ---                   | ---                     | ---                     |
| 2018-11-09 : 16:42    | CheckPositionValidity   | WaitHoldingTime         |
| 2018-11-09 : 16:58    | WaitHoldingTime         | CheckPositionValidity   |
| 2018-11-09 : 16:42    | CheckPositionValidity   | BuyState                |