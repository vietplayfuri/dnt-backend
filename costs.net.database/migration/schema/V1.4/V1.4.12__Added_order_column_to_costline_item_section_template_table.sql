alter table cost_line_item_section_template add column "order" int not null default(0);

update cost_line_item_section_template set 
"order" = 10
where name = 'postProduction';

update cost_line_item_section_template set 
"order" = 20
where name in ('agencyCosts');

update cost_line_item_section_template set 
"order" = 30
where name in ('otherCosts', 'OtherCosts', 'Other');