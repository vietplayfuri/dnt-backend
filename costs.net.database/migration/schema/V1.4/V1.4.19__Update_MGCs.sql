
-- Production, Audio, Direct to consumer
update pg_ledger_material_code
set material_group_code = 'G821215AA', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Audio') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Direct to consumer');

-- Production, Audio, In-store
update pg_ledger_material_code
set material_group_code = 'S821019BD', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Audio') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'In-store');


-- Production, Still Image, Direct to consumer
update pg_ledger_material_code
set material_group_code = 'G821215AA', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Photography') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Direct to consumer');

-- Production, Still Image, In-store
update pg_ledger_material_code
set material_group_code = 'G821215AA', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Photography') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'In-store');

-- Production, Still Image, PR/Influencer
update pg_ledger_material_code
set material_group_code = 'S82141500', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Photography') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'PR/Influencer');


-- Production, Video, Direct to consumer
update pg_ledger_material_code
set material_group_code = 'G821215AA', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Direct to consumer');

-- Production, Video, In-store
update pg_ledger_material_code
set material_group_code = 'S821019BD', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'In-store');

-- Production, Video, PR/Influencer
update pg_ledger_material_code
set material_group_code = 'S821019BD', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'PR/Influencer');





