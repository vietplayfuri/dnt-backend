
-- Buyout, Organization
update pg_ledger_material_code
set material_group_code = 'S80141903', modified = now()
where cost_type = 'Buyout' and 
usage_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'UsageType') AND de.key = 'Organization');

-- Buyout, Footage(stock pack-shots UGC  etc)
insert into pg_ledger_material_code (content_type_id, media_type_id, production_type_id, oval_id, material_group_code, general_ledger_code, general_ledger_code_multiple, 
	cost_type, usage_type_id, 
	created_by_id, created, modified)
SELECT null, null, null, null, 'S80141903', '33500003', null, 
'Buyout', (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'UsageType') AND de.key = 'Footage(stock pack-shots UGC  etc)'),
'77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now() 
WHERE
    NOT EXISTS (
        SELECT id FROM pg_ledger_material_code WHERE cost_type = 'Buyout' and 
        usage_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'UsageType') AND de.key = 'Footage(stock pack-shots UGC  etc)')
    );

-- Production, Audio, PR/Influencer
update pg_ledger_material_code
set material_group_code = 'S82141500', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Audio') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'PR/Influencer');

