
-- Add New MGCs here. Modify existing MGCs below

-- Production, Video, CGI, Cinema, Original
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Cinema'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Cinema') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original')
);

-- Production, Video, CGI, Cinema, Version
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Cinema'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Cinema') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version')
);

-- Production, Video, CGI, Cinema, Lift
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Cinema'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Cinema') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift')
);

-- Production, Video, CGI, Digital, Original
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Digital'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Digital') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original')
);

-- Production, Video, CGI, Digital, Version
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Digital'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Digital') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version')
);

-- Production, Video, CGI, Digital, Lift
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Digital'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Digital') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift')
);

-- Production, Video, CGI, Cinema, Lift
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Cinema'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Cinema') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift')
);

-- Production, Video, CGI, Direct to consumer, Original
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Direct to consumer'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original'),
'G821215AA', '33710001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Direct to consumer') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original')
);

-- Production, Video, CGI, Direct to consumer, Version
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Direct to consumer'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version'),
'G821215AA', '33710001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Direct to consumer') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version')
);

-- Production, Video, CGI, Direct to consumer, Lift
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Direct to consumer'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift'),
'G821215AA', '33710001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Direct to consumer') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift')
);


-- Production, Video, CGI, In-store, Original
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'In-store'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original'),
'S821019BD', '34420006', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'In-store') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original')
);

-- Production, Video, CGI, In-store, Version
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'In-store'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version'),
'S821019BD', '34420006', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'In-store') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version')
);

-- Production, Video, CGI, In-store, Lift
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'In-store'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift'),
'S821019BD', '34420006', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'In-store') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift')
);


-- Production, Video, CGI, Out of home, Original
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Out of home'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Out of home') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original')
);

-- Production, Video, CGI, Out of home, Version
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Out of home'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Out of home') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version')
);

-- Production, Video, CGI, Out of home, Lift
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Out of home'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Out of home') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift')
);


-- Production, Video, CGI, PR/Influencer, Original
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'PR/Influencer'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original'),
'S821019BD', '33720001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'PR/Influencer') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original')
);

-- Production, Video, CGI, PR/Influencer, Version
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'PR/Influencer'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version'),
'S821019BD', '33720001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'PR/Influencer') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version')
);

-- Production, Video, CGI, PR/Influencer, Lift
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'PR/Influencer'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift'),
'S821019BD', '33720001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'PR/Influencer') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift')
);


-- Production, Video, CGI, streaming audio, Original
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'streaming audio'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'streaming audio') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original')
);

-- Production, Video, CGI, streaming audio, Version
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'streaming audio'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'streaming audio') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version')
);

-- Production, Video, CGI, streaming audio, Lift
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'streaming audio'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'streaming audio') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift')
);

-- Production, Video, CGI, Tv, Original
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Tv'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Tv') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original')
);

-- Production, Video, CGI, Tv, Version
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Tv'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Tv') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version')
);

-- Production, Video, CGI, Tv, Lift
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Tv'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Tv') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift')
);

-- Production, Video, CGI, N/A, Original
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'N/A'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'N/A') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original')
);

-- Production, Video, CGI, N/A, Version
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'N/A'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'N/A') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version')
);

-- Production, Video, CGI, N/A, Lift
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'N/A'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'N/A') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift')
);

-- Production, Video, CGI, not for air, Original
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'not for air'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'not for air') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Original')
);

-- Production, Video, CGI, not for air, Version
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'not for air'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'not for air') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Version')
);

-- Production, Video, CGI, not for air, Lift
insert into pg_ledger_material_code (
content_type_id,
media_type_id,
production_type_id,
oval_id,
material_group_code, general_ledger_code, general_ledger_code_multiple, cost_type, usage_type_id, created_by_id, created, modified)
SELECT 
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'not for air'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift'),
'S821018AT', '33500001', null, 'Production', null, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
WHERE
	not exists (
		select id from pg_ledger_material_code
		where cost_type = 'Production' and 
		content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') and 
		media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'not for air') and 
		production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and 
		oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift')
);

-- Modify existing MGCs here

-- Production, Video, CGI, In-store, Lift
update pg_ledger_material_code
set material_group_code = 'S821019BD', general_ledger_code = '34420006', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Video') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'In-store') and
production_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ProductionType') AND de.key = 'CGI/Animation') and
oval_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'OvalType') and de.key = 'Lift');

-- Production, Digital, Digital  
update pg_ledger_material_code
set material_group_code = 'S821018AU', general_ledger_code = '33500001', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Digital') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Digital') and
production_type_id is null and oval_id is null;

-- Production, Digital, Multiple  
update pg_ledger_material_code
set material_group_code = 'S821018AU', general_ledger_code = '33500001', modified = now()
where cost_type = 'Production' and
content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Digital') AND
media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Multiple') and
production_type_id is null and oval_id is null;

-- Production, Digital, N/A
insert into pg_ledger_material_code (
	content_type_id,
	media_type_id,
	material_group_code,
	general_ledger_code,
	cost_type,
	created_by_id
)
SELECT
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Digital'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'N/A'),
'S821018AU',
'33500001',
'Production',
'77681eb0-fc0d-44cf-83a0-36d51851e9ae'
WHERE
	not exists (
		select 1 from pg_ledger_material_code
		where 
			cost_type = 'Production'
			and content_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'ContentType') AND de.key = 'Digital') 
			and media_type_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'N/A')
	);