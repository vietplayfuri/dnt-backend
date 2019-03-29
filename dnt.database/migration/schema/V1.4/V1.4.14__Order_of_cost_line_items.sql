update cost_line_item_section_template_item set 
"order" = ti.rnum * 10
from (
	select 
		t.id,
		row_number() OVER () as rnum
	from
		cost_line_item_section_template_item t
) as ti
where ti.id = cost_line_item_section_template_item.id;

update cost_line_item_section_template_item set 
"order" = ti.order - s.min
from cost_line_item_section_template_item ti
join 
(
	select 
		section_template_id, min("order") as min
	from cost_line_item_section_template_item group by section_template_id
) as s on s.section_template_id = ti.section_template_id
where ti.id = cost_line_item_section_template_item.id;