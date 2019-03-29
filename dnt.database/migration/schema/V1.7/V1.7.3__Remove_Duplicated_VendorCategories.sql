delete from vendor_category 
where id in (
	WITH category_view AS (
		select
			vc.id,
			vc.name,
			vc.vendor_id,
			row_number() OVER () as rnum
		from 
		vendor_category vc
		order by vc.vendor_id
	)
	select 
		v.id
	from
		category_view v
	join (
		select
			v1.vendor_id,
			v1.name,
            count(*) as cnt
		from 
		category_view v1
		group by v1.vendor_id, v1.name
	) as c on c.vendor_id = v.vendor_id and c.name = v.name
	left join (
		select
			v1.vendor_id,
			v1.name,
			min(v1.rnum) as min_rnum
		from 
		category_view v1
		group by v1.vendor_id, v1.name
	) as m on m.vendor_id = v.vendor_id and m.name = v.name and m.min_rnum = v.rnum
    where c.cnt > 1 and m.vendor_id is null
)
and not exists (select 1 from vendor_rule vr where vr.vendor_category_id = vendor_category.id);