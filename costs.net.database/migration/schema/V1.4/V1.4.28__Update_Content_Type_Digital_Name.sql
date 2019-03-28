update dictionary_entry 
set value = 'Digital Development'
where id = (select de.id from dictionary_entry de
join dictionary d on d.id = de.dictionary_id 
where d.name = 'ContentType' and de.key = 'Digital');