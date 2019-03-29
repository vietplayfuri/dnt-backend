

-- Audio, not for air, Full Production
insert into pg_ledger_material_code (content_type_id, media_type_id, production_type_id, oval_id, material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
values (
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Audio'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'not for air'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'Full Production'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Multiple'),
'S821018AU',
'33500001',
null,
'Production',
null,
'77681eb0-fc0d-44cf-83a0-36d51851e9ae',
now(),
now()
);

-- Audio, not for air, Post Production Only
insert into pg_ledger_material_code (content_type_id, media_type_id, production_type_id, oval_id, material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
values (
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Audio'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'not for air'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'Post Production Only'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Multiple'),
'S821018AU',
'33500001',
null,
'Production',
null,
'77681eb0-fc0d-44cf-83a0-36d51851e9ae',
now(),
now()
);

-- Buyout, Photography
update pg_ledger_material_code
set material_group_code = 'S55111500', modified = now()
where cost_type = 'Buyout' and 
usage_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'UsageType') AND de.key = 'Photography');

-- Buyout, Voice-Over
update pg_ledger_material_code
set material_group_code = 'S55111500', modified = now()
where cost_type = 'Buyout' and 
usage_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'UsageType') AND de.key = 'Voice-Over');

