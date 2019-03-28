alter table cost_stage_revision drop column owner_id;

drop view cost_line_item_view;

CREATE VIEW cost_line_item_view AS
 SELECT lineitem.id,
    lineitem.cost_stage_revision_id,
    lineitem.created_by_id,
    lineitem.created,
    lineitem.modified,
    lineitem.template_section_id,
    lineitem.name,
    template.name AS template_section_name,
    lineitem.value_in_default_currency,
    lineitem.value_in_local_currency,
    lineitem.local_currency_id
   FROM cost_line_item lineitem
     JOIN cost_line_item_section_template template ON template.id = lineitem.template_section_id;

alter table cost_line_item drop column owner_id;
