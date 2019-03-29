do $$
declare 
	tmp_rules t_rule[] = array[]::t_rule[];
	rule_type int2 = 9;
begin
	perform fn_update_business_rules(
		rule_type,
		tmp_rules ||
		(
			'OriginalEstimate Production Excluding Agency In NORTHERN_AMERICA_AREA(Mandatory Docs)',
			rule_type,
			'{
				"Name": "Supplier winning bid (budget form)",
				"Key": "BudgetForm",
				"CanManuallyUpload": false
			}',
			'{
				"FieldName": null,
				"Operator": "And",
				"TargetValue": null,
				"Children": [
					{
						"FieldName": null,
						"Operator": "And",
						"TargetValue": null,
						"Children": [
							{
								"FieldName": "BudgetRegion",
								"Operator": "NotEqual",
								"TargetValue": "NORTHERN AMERICA AREA",
								"Children": []
							},
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Production",
								"Children": []
							},
							{
								"FieldName": null,
								"Operator": "Or",
								"TargetValue": null,
								"Children": [
									{
										"FieldName": "CostStage",
										"Operator": "Equal",
										"TargetValue": "Aipe",
										"Children": []
									},
									{
										"FieldName": "CostStage",
										"Operator": "Equal",
										"TargetValue": "OriginalEstimate",
										"Children": []
									}
								]
							}
						]
					}
				]
			}',
			0
		)::t_rule ||
		(
			'OriginalEstimate Production For Agency In NORTHERN_AMERICA_AREA(Optional Docs)',
			rule_type,
			'{
				"Name": "Supplier winning bid (budget form)",
				"Key": "BudgetForm",
				"Mandatory": false,
				"CanManuallyUpload": false
			}',
			'{
				"FieldName": null,
				"Operator": "And",
				"TargetValue": null,
				"Children": [
					{
						"FieldName": null,
						"Operator": "And",
						"TargetValue": null,
						"Children": [
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "NORTHERN AMERICA AREA",
								"Children": []
							},
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Production",
								"Children": []
							},
							{
								"FieldName": null,
								"Operator": "Or",
								"TargetValue": null,
								"Children": [
									{
										"FieldName": "CostStage",
										"Operator": "Equal",
										"TargetValue": "Aipe",
										"Children": []
									},
									{
										"FieldName": "CostStage",
										"Operator": "Equal",
										"TargetValue": "OriginalEstimate",
										"Children": []
									}
								]
							}
						]
					}
				]
			}',
			0
		)::t_rule ||
		(
			'OriginalEstimate FullProduction Audio&Video(Mandatory Docs)',
			rule_type,
			'{
				"Name": "Approved Creative (storyboard/layout/script)"
			}',
			'{
				"FieldName": null,
				"Operator": "And",
				"TargetValue": null,
				"Children": [
					{
						"FieldName": null,
						"Operator": "And",
						"TargetValue": null,
						"Children": [
							{
								"FieldName": "CostStage",
								"Operator": "Equal",
								"TargetValue": "OriginalEstimate",
								"Children": []
							},
							{
								"FieldName": "ProductionType",
								"Operator": "Equal",
								"TargetValue": "Full Production",
								"Children": []
							},
							{
								"FieldName": null,
								"Operator": "Or",
								"TargetValue": null,
								"Children": [
									{
										"FieldName": "ContentType",
										"Operator": "Equal",
										"TargetValue": "Audio",
										"Children": []
									},
									{
										"FieldName": "ContentType",
										"Operator": "Equal",
										"TargetValue": "Video",
										"Children": []
									}
								]
							},
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Production",
								"Children": []
							}
						]
					}
				]
			}',
			0
		)::t_rule ||
		(
			'OriginalEstimate Production(Mandatory Docs)',
			rule_type,
			'{
				"Name": "P&G Communication Brief"
			}',
			'{
				"FieldName": null,
				"Operator": "Or",
				"TargetValue": null,
				"Children": [
					{
						"FieldName": null,
						"Operator": "And",
						"TargetValue": null,
						"Children": [
							{
								"FieldName": "CostStage",
								"Operator": "Equal",
								"TargetValue": "OriginalEstimate",
								"Children": []
							},
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Production",
								"Children": []
							}
						]
					},
					{
						"FieldName": null,
						"Operator": "And",
						"TargetValue": null,
						"Children": [
							{
								"FieldName": "CostStage",
								"Operator": "Equal",
								"TargetValue": "Aipe",
								"Children": []
							},
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Production",
								"Children": []
							}
						]
					}
				]
			}',
			0
		)::t_rule ||
		(
			'OriginalEstimate or FinalActual when NewCost Usage&Buyout(Mandatory Docs)',
			rule_type,
			'{
				"Name": "Brief/Call for work"
			}',
			'{
			   "Operator": "And",
				"Children": [{
					"Operator": "Equal",
					"FieldName": "CostType",
					"TargetValue": "Buyout"
				},{
					"Operator": "Equal",
					"FieldName": "PreviousCostStage",
					"TargetValue": ""
				},
				{
					"Operator": "Or",
					"Children": [{
						"Operator": "Equal",
						"FieldName": "CostStage",
						"TargetValue": "OriginalEstimate"
					},{
						"Operator": "Equal",
						"FieldName": "CostStage",
						"TargetValue": "FinalActual"
					}]
				}]			
			}',
			0
		)::t_rule ||
		(
			'FirstPresentation Production Audio&Video(Mandatory Docs)',
			rule_type,
			'{
				"Name": "First Presentation Confirmation Email"
			}',
			'{
				"FieldName": null,
				"Operator": "And",
				"TargetValue": null,
				"Children": [
					{
						"FieldName": null,
						"Operator": "And",
						"TargetValue": null,
						"Children": [
							{
								"FieldName": "CostStage",
								"Operator": "Equal",
								"TargetValue": "FirstPresentation",
								"Children": []
							},
							{
								"FieldName": null,
								"Operator": "Or",
								"TargetValue": null,
								"Children": [
									{
										"FieldName": "ContentType",
										"Operator": "Equal",
										"TargetValue": "Video",
										"Children": []
									},
									{
										"FieldName": "ContentType",
										"Operator": "Equal",
										"TargetValue": "Photography",
										"Children": []
									}
								]
							},
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Production",
								"Children": []
							}
						]
					}
				]
			}',
			0
		)::t_rule ||
		(
			'FinalActual Production(Mandatory Docs)',
			rule_type,
			'{
				"Name": "Final (online) version approval from brand"
			}',
			'{
				"FieldName": null,
				"Operator": "And",
				"TargetValue": null,
				"Children": [
					{
						"FieldName": null,
						"Operator": "And",
						"TargetValue": null,
						"Children": [
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Production",
								"Children": []
							},
							{
								"FieldName": "CostStage",
								"Operator": "Equal",
								"TargetValue": "FinalActual",
								"Children": []
							}
						]
					}
				]
			}',
			0
		)::t_rule ||
		(
			'FinalActual Usage&Buyout(Mandatory Docs)',
			rule_type,
			'{
				"Name": "Signed contract or letter of extension"
			}',
			'{
				"FieldName": null,
				"Operator": "And",
				"TargetValue": null,
				"Children": [
					{
						"FieldName": null,
						"Operator": "And",
						"TargetValue": null,
						"Children": [
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Buyout",
								"Children": []
							},
							{
								"FieldName": "CostStage",
								"Operator": "Equal",
								"TargetValue": "FinalActual",
								"Children": []
							}
						]
					}
				]
			}',
			0
		)::t_rule ||
		(
			'Revisions(Mandatory Docs)',
			rule_type,
			'{
				"Name": "Scope change approval form"
			}',
			'{
				"Children": [{
					"Children": [{
						"Operator": "Equal",
						"FieldName": "CostStage",
						"TargetValue": "OriginalEstimateRevision"
					},
					{
						"Operator": "Equal",
						"FieldName": "CostStage",
						"TargetValue": "FirstPresentationRevision"
					}],
					"Operator": "Or"
				},
				{
					"Children": [{
						"Children": [{
							"Operator": "Equal",
							"FieldName": "CostStage",
							"TargetValue": "FirstPresentation"
						},
						{
							"Operator": "Equal",
							"FieldName": "CostStage",
							"TargetValue": "FinalActual"
						}],
						"Operator": "Or"
					},
					{
						"Children": [{
							"Operator": "Equal",
							"FieldName": "CostType",
							"TargetValue": "Production"
						},
						{
							"Operator": "Equal",
							"FieldName": "CostType",
							"TargetValue": "Buyout"
						}],
						"Operator": "Or"
					},
					{
						"Operator": "Equal",
						"FieldName": "TotalCostIncreased",
						"TargetValue": "true"
					}],
					"Operator": "And"
				}],
				"Operator": "Or"
			}',
			0
		)::t_rule
	);
end 
$$ language plpgsql;	
