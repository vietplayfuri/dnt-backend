do $$
declare 
	tmp_rules t_rule[] = array[]::t_rule[];
	rule_type int2 = 2;
begin
	perform fn_update_business_rules(
		rule_type,
		tmp_rules ||

        -- RULE FOR ADC-574
        -- RULE FOR ADC-575
        -- RULE FOR ADC-576
		(
			'AIPEApplicabilityRule1',
			rule_type,
			'{}',
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
								"FieldName": "TargetBudgetAmount",
								"Operator": "GreaterThanOrEqual",
								"TargetValue": "0.0",
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
								"FieldName": null,
								"Operator": "Or",
								"TargetValue": null,
								"Children": [
									{
										"FieldName": "BudgetRegion",
										"Operator": "Equal",
										"TargetValue": "NORTHERN AMERICA AREA",
										"Children": []
									},
									{
										"FieldName": "BudgetRegion",
										"Operator": "Equal",
										"TargetValue": "AAK (Asia)",
										"Children": []
									},
									{
										"FieldName": "BudgetRegion",
										"Operator": "Equal",
										"TargetValue": "EUROPE AREA",
										"Children": []
									},
									{
										"FieldName": "BudgetRegion",
										"Operator": "Equal",
										"TargetValue": "INDIA & MIDDLE EAST AFRICA AREA",
										"Children": []
									}
								]
							}
						]
					},
					{
						"FieldName": null,
						"Operator": "And",
						"TargetValue": null,
						"Children": [
							{
								"FieldName": null,
								"Operator": "Or",
								"TargetValue": null,
								"Children": [
									{
										"FieldName": "BudgetRegion",
										"Operator": "Equal",
										"TargetValue": "NORTHERN AMERICA AREA",
										"Children": []
									},
									{
										"FieldName": "BudgetRegion",
										"Operator": "Equal",
										"TargetValue": "AAK (Asia)",
										"Children": []
									},
									{
										"FieldName": "BudgetRegion",
										"Operator": "Equal",
										"TargetValue": "EUROPE AREA",
										"Children": []
									},
									{
										"FieldName": "BudgetRegion",
										"Operator": "Equal",
										"TargetValue": "INDIA & MIDDLE EAST AFRICA AREA",
										"Children": []
									}
								]
							},
							{
								"FieldName": "TargetBudgetAmount",
								"Operator": "GreaterThanOrEqual",
								"TargetValue": "50000.0",
								"Children": []
							},
							{
								"FieldName": null,
								"Operator": "Or",
								"TargetValue": null,
								"Children": [
									{
										"FieldName": "ProductionType",
										"Operator": "Equal",
										"TargetValue": "Post Production Only",
										"Children": []
									},
									{
										"FieldName": "ProductionType",
										"Operator": "Equal",
										"TargetValue": "CGI/Animation",
										"Children": []
									}
								]
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
							}
						]
					},
					{
						"FieldName": null,
						"Operator": "And",
						"TargetValue": null,
						"Children": [
							{
								"FieldName": null,
								"Operator": "Or",
								"TargetValue": null,
								"Children": [
									{
										"FieldName": "BudgetRegion",
										"Operator": "Equal",
										"TargetValue": "NORTHERN AMERICA AREA",
										"Children": []
									},
									{
										"FieldName": "BudgetRegion",
										"Operator": "Equal",
										"TargetValue": "AAK (Asia)",
										"Children": []
									},
									{
										"FieldName": "BudgetRegion",
										"Operator": "Equal",
										"TargetValue": "EUROPE AREA",
										"Children": []
									},
									{
										"FieldName": "BudgetRegion",
										"Operator": "Equal",
										"TargetValue": "INDIA & MIDDLE EAST AFRICA AREA",
										"Children": []
									}
								]
							},
							{
								"FieldName": "TargetBudgetAmount",
								"Operator": "GreaterThanOrEqual",
								"TargetValue": "50000.0",
								"Children": []
							},
							{
								"FieldName": null,
								"Operator": "Or",
								"TargetValue": null,
								"Children": [
									{
										"FieldName": "ProductionType",
										"Operator": "Equal",
										"TargetValue": "Post Production Only",
										"Children": []
									},
									{
										"FieldName": "ProductionType",
										"Operator": "Equal",
										"TargetValue": "Full Production",
										"Children": []
									}
								]
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Audio",
								"Children": []
							}
						]
					}
				]
			}',
			0
		)::t_rule
	);
end 
$$ language plpgsql;	