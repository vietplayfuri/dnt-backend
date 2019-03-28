
insert into dependent_item (child_id, parent_id, created)
select
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Tv'),
(SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'UsageBuyoutType') AND de.key = 'Buyout'),
now()
WHERE
	not exists (
		select id from dependent_item
		where 
		parent_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'UsageBuyoutType') AND de.key = 'Buyout') and 
		child_id = (SELECT de.ID FROM dictionary_entry de WHERE de.dictionary_id = (SELECT ID FROM dictionary d WHERE d.name = 'MediaType/TouchPoints') AND de.key = 'Tv')
);
