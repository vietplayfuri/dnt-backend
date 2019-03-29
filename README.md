# ac.net

Seedy will generate a base set of agencies and Users in the gdam mongo DB it is configured for. Additionally it will import these users and agencies into the AdCost DB it is configured for.

Once data is imported into the AdCost DB it will populate the ACL service setting up the required roles and permissions for AdCosts as well as setting up the required parent/child relations ships and user groups for all agencies and users.

CONFIG
------

```xml
<add key="GdamMongoHost" value="mongodb://10.44.243.139:27017/?safe=true" />
<add key="GdamDBName" value="gdam" />
<add key="AdcostsMongoHost" value="mongodb://10.44.243.185:27017/?safe=true" />
<add key="AdcostsDBName" value="adcosts" />
<add key="AdminUser" value="4ef31ce1766ec96769b399c0" />
<add key="GdamCoreHost" value="http://10.44.243.185:8080" />
<add key="AclHost" value="http://10.44.243.12:7777/" />
```

Agencies and Users
----
* **PG UK1**
	* user1@pguk1.com
	* user2@pguk1.com
* **PG UK2**
	* user1@pguk2.com
	* user2@pguk2.com
* **PG USA**
	* user1@pgusa1.com
	* user2@pguas1.com
* **PG Thailand**
	* user1@pgthailand.com
	* user2@pgthailand.com
* **PG Indonesia**
	* user1@pgindonesia.com
	* user2@pgindonesia.com
* **Saatchi**
	* user1@saatchi1.com
	* user2@saatchi1.com 
# costs.net


.NET Core backend service for Costs. Provides a set of RESTful endpoints as well as an AMQ bus for consuming and producing events for A5. Requires Visual Studio 2015. 

### Solution
---
* **costs.net.api**
	* API routes
* **costs.net.consumers**
	* AMQ message bus for consuming and producing events for A5
* **costs.net.core**
	* Entity definitions, Helper classes, Interfaces, Business Logic
* **costs.net.host**
	* Main entry point for the app. Simple console runner. 
* **costs.net.plugins**
	* Where any bespoke client logic will be placed. E.g. P&G specific logic.
* **costs.net.scheduler**
	* Scheduled tasks using [FluentScheduler](https://github.com/fluentscheduler/FluentScheduler) e.g. sending approval email reminders.
* **costs.net.tests**
	* Unit and Integration tests for the project



### Deployment - QA/Dev
---
* **Environment grid**
    * http://qa-grid1.adstreamdev.com:9001/stackinfo/qa-a5ai1-29
* **costs.net build**
	* [Jenkins costs.net job](https://jenkins-internal.adstream.com/job/costs-net-build/) 
* **costs.net container job**
	* [Jenkins costs.net container job](https://jenkins-internal.adstream.com/job/costs-net-container/)
* **Deploy ACL server**
    * SSH to qa-a5-allinone-docker-qa-a5ai1-29.adstream.dev
    * **OPTIONAL:** Edit ``/opt/docker-compose.yml`` to change version of ACL server
    * ``cd /opt``
    * ``sudo docker-compose stop``
    * ``sudo rm -rf /opt/orientdb``
    * ``sudo docker-compose rm -vf``
    * ``sudo docker-compose up -d``
    * Recreate the database using Postman
	    * ``POST: http://localhost:7777/acl/v1/admin/db/acl``
* **Deploy A5 instance**
    * https://deploy-internal.adstream.com/job/QA-ALL-IN-ONE/job/provision-qa-allinone-stack/
    * SSH into qa-a5-allinone-qa-a5ai1-29.adstream.dev and edit ``/opt/front/middleware/local.config.js`` and set ``useSSO: false``
* **Costs A5 Instance**
	* [http://qa-a5-allinone-qa-a5ai1-29.adstream.dev](http://qa-a5-allinone-qa-a5ai1-29.adstream.dev)
	

### Startup Projects

* **costs.net.host** - The main Adcosts backend ASP.Net Core web app.
* **costs.net.seedy.api** - ASP.Net Core web app used to seed data into Adcosts database and A5 Core
* **costs.net.scheduler.host** - .Net Core console/daemon app used to run jobs on a cron

## costs.net.scheduler.host
At any one time, there should be only one costs.net.scheduler.host running across all Adcosts AWS hosts.

### Scheduled Jobs
The scheduler contains two jobs:  

#### [Email Notification Reminder](costs.net.scheduler.core/Jobs/EmailNotificationReminderJob.cs)
Every ten minutes, this job will check the [email_notification_reminder](costs.net.database/migration/schema/V1.0/V1.0.0__Initial.sql) table for reminders and send them synchronously.  

#### [Purge](costs.net.scheduler.core/Jobs/PurgeJob.cs)
At midnight, every day, this job will clear the [email_notification_reminder](costs.net.database/migration/schema/V1.0/V1.0.0__Initial.sql) table of rows where the status is 2 or 3 (cancelled or sent).

#### [Activity Log Delivery](costs.net.scheduler.core/Jobs/ActivityLogDeliveryJob.cs)
Every minute, this job will check the [activity_log_deliver_status](costs.net.database/migration/schema/V1.1/V1.1.39__Create_activity_log_delivery_table.sql) table for monitoring activity log entries and send them synchronously.
  
### Email Notification Reminder Database details
The [email_notification_reminder](costs.net.database/migration/schema/V1.0/V1.0.0__Initial.sql) table contains a status column. The column type is int. Here is what each number means:

* 0 - Reminder has not been sent.  
* 1 - Reminder is being processed and will be sent once the paper pusher payload message has been constructed.  
* 2 - Reminder is cancelled and will not be sent. This can happen when a cost has been approved, rejected or recalled.  
* 3 - Reminder has been successfully sent.  

For testing purposes, you can change the status column to re-send reminders or change the reminder_timestamp to adjust when a reminder should be sent.

Every day at midnight a purge job will run to delete the rows in the [email_notification_reminder](costs.net.database/migration/schema/V1.0/V1.0.0__Initial.sql) table with status 2 or 3.

### Activity Log Delivery Database details
The [activity_log_deliver_status](costs.net.database/migration/schema/V1.1/V1.1.39__Create_activity_log_delivery_table.sql) table contains a status column. The column type is int. Here is what each number means:

* 0 - Activity Log entry has not been sent.
* 1 - Activity Log entry is being processed and will be sent once the paper pusher payload message has been constructed.
* 2 - Activity Log entry has been successfully sent to paper pusher, i.e. paper pusher returns HTTP Status 200 OK.
* 3 - Activity Log entry has failed and will be re-tried later.
* 4 - Activity Log entry has reached the maximum number of re-tries and a support email has been sent about this issue.
