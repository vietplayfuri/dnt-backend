insert into dictionary_entry (dictionary_id, "key", value, visible, created, modified)
values ((select d.id from dictionary d where d."name" = 'VendorCategory'), 'UsageBuyoutContract','Usage/buyout/contract', true, now(), now());

insert into dictionary_entry (dictionary_id, "key", value, visible, created, modified)
values ((select d.id from dictionary d where d."name" = 'VendorCategory'), 'DistributionTrafficking','Distribution & Trafficking', true, now(), now());
