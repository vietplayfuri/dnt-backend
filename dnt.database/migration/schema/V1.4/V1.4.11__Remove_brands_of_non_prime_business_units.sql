delete from brand 
where id in(
	select b.id 
	from brand b
	join agency a on a.id = b.agency_id
	where not a.labels @> array['CM_Prime_P&G']
	)