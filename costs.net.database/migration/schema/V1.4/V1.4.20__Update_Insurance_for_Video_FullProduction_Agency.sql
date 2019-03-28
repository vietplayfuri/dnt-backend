UPDATE cost_line_item_section_template_item
  SET label = 'Insurance'
  WHERE id IN (SELECT cti.id
    FROM cost_line_item_section_template_item cti
    INNER JOIN cost_line_item_section_template ct ON cti.section_template_id = ct.id AND lower(ct."name") = 'agencycosts'
    INNER JOIN form_definition fd ON ct.form_definition_id = fd.id AND lower(fd."name") = 'fullproductionwithshoot'
    WHERE cti."name" = 'insurance');