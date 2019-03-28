
DELETE from user_user_group where user_group_id in (select ug.id from user_group ug
where ug.object_type = 'cost' and ug.object_id not in (select id from cost));

delete from user_group
where id in (select ug.id from user_group ug
where ug.object_type = 'cost' and ug.object_id not in (select id from cost));