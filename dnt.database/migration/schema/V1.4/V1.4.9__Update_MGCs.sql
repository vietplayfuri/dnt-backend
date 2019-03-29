
-- Production, Audio, Direct to consumer
update pg_ledger_material_code
set material_group_code = 'S821018AU', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Audio') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Direct to consumer');

-- Production, Audio, In-store
update pg_ledger_material_code
set material_group_code = 'S821018AU', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Audio') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'In-store');

-- Trafficking
update pg_ledger_material_code
set material_group_code = 'S821018AT', general_ledger_code = '33500002', modified = now()
where cost_type = 'Trafficking';

-- Production, Still Image, Direct to consumer
update pg_ledger_material_code
set material_group_code = 'S821018AU', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Photography') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Direct to consumer');

-- Production, Still Image, In-store
update pg_ledger_material_code
set material_group_code = 'S821018AU', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Photography') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'In-store');

-- Production, Still Image, PR/Influencer
update pg_ledger_material_code
set material_group_code = 'S821018AU', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Photography') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'PR/Influencer');

-- Buyout, Celebrity
update pg_ledger_material_code
set material_group_code = 'S80141903', general_ledger_code = '33500003', modified = now()
where cost_type = 'Buyout' and 
usage_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'UsageType') AND de.key = 'Celebrity');

-- Buyout, Athletes
update pg_ledger_material_code
set material_group_code = 'S80141903', general_ledger_code = '33500003', modified = now()
where cost_type = 'Buyout' and 
usage_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'UsageType') AND de.key = 'Athletes');

-- Buyout, Music
update pg_ledger_material_code
set material_group_code = 'S55111500', general_ledger_code = '33500004', modified = now()
where cost_type = 'Buyout' and 
usage_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'UsageType') AND de.key = 'Music');

-- Production, Video, Direct to consumer
update pg_ledger_material_code
set material_group_code = 'S821018AU', general_ledger_code = '33710001', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Direct to consumer');

-- Production, Video, In-store
update pg_ledger_material_code
set material_group_code = 'S821018AU', general_ledger_code = '34420006', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'In-store');

-- Production, Video, PR/Influencer
update pg_ledger_material_code
set material_group_code = 'S821018AU', general_ledger_code = '33720001', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'PR/Influencer');

-- Production, Digital, Digital
update pg_ledger_material_code
set material_group_code = 'S821018AU', general_ledger_code = '33500001', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Digital') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Digital');



