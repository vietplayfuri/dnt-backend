/* 
1 - When AdCosts generates supporting documents, it bases on conditions to decide whether or not the Budget form is generated
2 - Budget forms at First Presentation and FinalActual are optional
*/

insert into "rule" (name, "type", definition, criteria, priority, created, created_by_id, modified)
select 'FinalActual and FirstPresentation budget form (Optional Docs)', 9, '{"Key": "BudgetForm", "Name": "Supplier winning bid (budget form)", "Mandatory": false, "CanManuallyUpload": false}', '{"Children": [{"Children": [{"Children": [], "Operator": "NotEqual", "FieldName": "BudgetRegion", "TargetValue": "NORTHERN AMERICA AREA"}, {"Children": [], "Operator": "Equal", "FieldName": "CostType", "TargetValue": "Production"}, {"Children": [{"Children": [], "Operator": "Equal", "FieldName": "CostStage", "TargetValue": "Aipe"}, {"Children": [], "Operator": "Equal", "FieldName": "CostStage", "TargetValue": "FinalActual"}, {"Children": [], "Operator": "Equal", "FieldName": "CostStage", "TargetValue": "FirstPresentation"}], "Operator": "Or", "FieldName": null, "TargetValue": null}], "Operator": "And", "FieldName": null, "TargetValue": null}], "Operator": "And", "FieldName": null, "TargetValue": null}', 0, now(), '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now()
where not exists (select "name" from "rule" where name = 'FinalActual and FirstPresentation budget form (Optional Docs)');