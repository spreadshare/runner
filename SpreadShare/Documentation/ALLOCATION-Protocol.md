## Solved Problems

### When is a trade succesful?
A trade is considered succesful when the exchange trading provider reports it as such. 


### Who checks the exchange trading provider?
The trading provider will be checked by the allocation manager, who using real world data, has a
final say over the state of the portfolio.

### How does the allocaction provider learn about a trade?
The trading provider will inform its weak allocation manager about a trade that was reported
as succesful by its implementation.

### What happens when the local and remote portfolio diverge?
The remote portfolio is considered more truthful, but a warning is produced.

### How does the allocation provider handle updates? (and differentiate between algorithms)
* The weak allocation provider will inform the actual allociation provider service about a change that
the trading provider reported. The allocation service will use this trade to create a diverging branch
of the portfolio state. 

* Subsequently, it will fetch the actual portfolio (using the eponymous service), to generate a second branched
state.

* For the final stage, the newly fetched remote portfolio will be checked against the original known state. If no diff
is occured, the application will produce a warning, but continue with the remote portfolio as truth.

* Lastly, if the previous step did produce a diff, the two branches (remote, and supposedly after trade) will be
checked againt each other. The ideal is that these version are on and the same. Yet, when they are not, another
warning is produces and the remote portfolio considered the truth.

### How does the allocation manager interpret the remote portfolio back to a, per algorithm, splitted list?
Trading providers are kept in a busy wait (when requesting trade verification) until no other trades are pending. This ensures
to remote state to diff by a maximum of one trade. It can now be assumed that any diff occurs in the allocation
of the algorithm that reported the trade in the first place.


