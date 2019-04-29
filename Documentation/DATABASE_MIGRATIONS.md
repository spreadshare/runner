## Abstract
This document describes the basics of handling databases, and more specifically: migrations, in both development and production environments.

## Case 1: development
There are very few wrong answers here, the point is to be able to reliably generate a database and migrate the hell out of it back and forth while editing code.
The two obvious ways are installing postgres the normal way via apt, but personally I prefer running a posgres docker image, to leave as little as possible traces
in my main system. It also has the added benefit of easy disposal when things go very south.

## Case 2: Production

### Non breaking and idempotent, bullshit
Never trust the ORM to generate the desired and safe scripts; even when only 'just adding an extra column without constraints'. It is of great importance
that one always tests migrations in a safe and simulated environment.

### Create a simulation environment
For this we need a backup of the production database, preferably recent. Such a backup can be easily obtained by running

```
pg_dump -d [database] > [backup_file]
# Please not that this operation will lock the entire database, this may cause a short denial of service
# Usually this process takes a matter of miliseconds and carries no serious rammifications, but please use
# common sense.
```

From that point on it is just a matter of copying that file to your own machiine. Here you firstly have to create a phony database. This can be done by 
running the following statement from the psql console.

```
CREATE DATABASE [migration_test_db];
```

Now that the database exists, we can instruct postgres to restore the database from our backup:

```
psql -U [role] -d [migration_test_db] -f [backup_file]
```

If you are using a docker image, simpy run `docker exec -it [container] /bin/bash` to get access to the environment. (Also enlist the help from `docker cp` to get access to your backup file.)

### Testing the migration

Now that we have an environment that mimics the current state of the production database to the best of our abilities, we can start our test. Please do not forget to adjust any connection
strings to reflect our recently created database as target.

**1. Inspect the migration script**<br/>
How attractive the everworking EF CORE overlords may appear, their authority means nothing to your average system manager. It is important to always
check the raw migration scripts for any irregularities, misunderstandings or just plain bugs. The migration script can be obtained as follows:

```
dotnet ef migrations script [FROM] [TO]
```

The [TO] field defaults to the most recent migration if unspecified (also applies to step 2).

**2. Run the migration**<br/>
This can simply be done by running

```
dotnet ef database update [TO]
```

This process might fail if conflicts arise, if this is the case proceed to step 4.

**3. Inspecting the resutls**<br/>
Time to get a good look at the results, fire up your favourite db browser, test suite or any other means that can help you get some assurance as to the sucessful evolution to a new world.  
Be sure to visit any table and view, making sure they are all available and no query produces any errors. Also make sure that any data that is caused to be lost is non essential or otherwise
backed up. 

**4. Fixing problems**<br/>
Some migrations require some aftercare, or even some prework. In all likelyhood, a simply python script would be adequite to design any operational logic needed to forge the database into the desired situation, either to accomodate a migration or to copy, populate, or derive column values to ensure continued operability moving forward. If any actions in this step had to be taken, make sure to restore the database once more from backup and replay all the previous steps to ensure completeness. 

**5. Finishing up**<br/>
If all goes well, you should now be ready to replay the carefully designed set of operations. Just as a friendly reminder
- If the migration has to be finished on a strict deadline, don't do it
- If you are feeling not 100%, don't do it
- Ask for help and advice

