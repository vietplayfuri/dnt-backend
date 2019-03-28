do $$
declare 
	tmp_rules t_rule[] = array[]::t_rule[];
	rule_type int2 = 6;
begin
	perform fn_update_business_rules(
		rule_type,
		tmp_rules ||
		(
			'ChinaApprovalRule-ADC-2559-1',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "false",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"Operator": "And",
						"Children": [
							{
								"FieldName": "TotalCostAmount",
								"Operator": "GreaterThanOrEqual",
								"TargetValue": "0.0"
							},
							{
								"FieldName": "TotalCostAmount",
								"Operator": "LessThan",
								"TargetValue": "10000.0"
							},
							{
								"Operator": "Or",
								"Children": [
									{
										"FieldName": "CostType",
										"Operator": "Equal",
										"TargetValue": "Production"
									},
									{
										"FieldName": "CostType",
										"Operator": "Equal",
										"TargetValue": "Buyout"
									}
								]
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "GREATER CHINA AREA"
							}
						]
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'ChinaApprovalRule-ADC-2559-2',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "true",
				"IpmApprovalEnabled": "true",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "10000.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "100000.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Production"
							},
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Buyout"
							}
						]
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "GREATER CHINA AREA"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'ChinaApprovalRule-ADC-2559-3',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "true",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "100000.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Production"
							},
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Buyout"
							}
						]
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "GREATER CHINA AREA"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'AAKApprovalRule-ADC-855-1',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "false",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "10000.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Audio"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Digital"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Photography"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Video"
							},
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Buyout"
							}
						]
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "AAK (Asia)"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'AAKApprovalRule-ADC-855-2',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "true",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "10000.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Audio"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Digital"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Photography"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Video"
							},
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Buyout"
							}
						]
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "AAK (Asia)"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'JapanApprovalRule-ADC-856-1',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "false",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "10000.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Audio"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Digital"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Photography"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Video"
							},
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Buyout"
							}
						]
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "JAPAN"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'JapanApprovalRule-ADC-856-2',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "true",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "10000.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Audio"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Digital"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Photography"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Video"
							},
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Buyout"
							}
						]
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "JAPAN"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'EuropeIMEAApprovalRule-ADC-2559',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "true",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "100000.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Buyout"
							},
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Production"
							}
						]
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "EUROPE AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "INDIA & MIDDLE EAST AFRICA AREA"
							}
						]
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'EuropeIMEAApprovalRule-ADC-2559',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "true",
				"IpmApprovalEnabled": "true",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "10000.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "100000.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Buyout"
							},
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Production"
							}
						]
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "EUROPE AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "INDIA & MIDDLE EAST AFRICA AREA"
							}
						]
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'Distribution-ADC-560',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "false",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "CostType",
						"Operator": "Equal",
						"TargetValue": "Trafficking"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "IsCyclone",
								"Operator": "Equal",
								"TargetValue": "false"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "NotEqual",
								"TargetValue": "NORTHERN AMERICA AREA"
							}
						]
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		-- ADC-2075
		(
			'Distribution-Cyclone-NA',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "false",
				"BrandApprovalEnabled": "true",
				"HasExternalIntegration": "false"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "CostType",
						"Operator": "Equal",
						"TargetValue": "Trafficking"
					},
					{
						"FieldName": "IsCyclone",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "NORTHERN AMERICA AREA"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'EuropeIMEAApprovalRule-ADC-2559',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "false",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "10000.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "EUROPE AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "INDIA & MIDDLE EAST AFRICA AREA"
							}
						]
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'LatAmApprovalRule-ADC-2559-A-1',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "true",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "100000.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Buyout"
							},
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Production"
							}
						]
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "LATIN AMERICA AREA"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'LatAmApprovalRule-ADC-2559-B-1',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "true",
				"IpmApprovalEnabled": "true",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "10000.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "100000.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Buyout"
							},
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Production"
							}
						]
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "LATIN AMERICA AREA"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'LatAmApprovalRule-ADC-2559-C-1',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "false",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "10000.0"
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "LATIN AMERICA AREA"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'NAApprovalRule-ADC-2559-A-1c',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "true",
				"BrandApprovalEnabled": "true",
				"HasExternalIntegration": "false"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsCyclone",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "CostType",
						"Operator": "Equal",
						"TargetValue": "Production"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "100000.0"
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "NORTHERN AMERICA AREA"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'NAApprovalRule-ADC-1130-A-2',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "true",
				"BrandApprovalEnabled": "true",
				"HasExternalIntegration": "false"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsCyclone",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "100000.0"
					},
					{
						"FieldName": "CostType",
						"Operator": "Equal",
						"TargetValue": "Buyout"
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "NORTHERN AMERICA AREA"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'NAApprovalRule-ADC-2559-B-1c',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "true",
				"IpmApprovalEnabled": "true",
				"BrandApprovalEnabled": "true",
				"HasExternalIntegration": "false"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsCyclone",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "CostType",
						"Operator": "Equal",
						"TargetValue": "Production"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "10000.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "100000.0"
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "NORTHERN AMERICA AREA"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'NAApprovalRule-ADC-1130-B-2',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "true",
				"IpmApprovalEnabled": "true",
				"BrandApprovalEnabled": "true",
				"HasExternalIntegration": "false"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsCyclone",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "10000.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "100000.0"
					},
					{
						"FieldName": "CostType",
						"Operator": "Equal",
						"TargetValue": "Buyout"
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "NORTHERN AMERICA AREA"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'NAApprovalRule-ADC-1130-C-1',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "false",
				"BrandApprovalEnabled": "true",
				"HasExternalIntegration": "false"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsCyclone",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "10000.0"
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "NORTHERN AMERICA AREA"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'NAApprovalRule-ADC-2559-A-1',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "true",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsCyclone",
						"Operator": "Equal",
						"TargetValue": "false"
					},
					{
						"FieldName": "CostType",
						"Operator": "Equal",
						"TargetValue": "Production"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "100000.0"
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "NORTHERN AMERICA AREA"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'NAApprovalRule-ADC-1131-A-2',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "true",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsCyclone",
						"Operator": "Equal",
						"TargetValue": "false"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "100000.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Buyout"
							}
						]
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "NORTHERN AMERICA AREA"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'NAApprovalRule-ADC-2559-B-1',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "true",
				"IpmApprovalEnabled": "true",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsCyclone",
						"Operator": "Equal",
						"TargetValue": "false"
					},
					{
						"FieldName": "CostType",
						"Operator": "Equal",
						"TargetValue": "Production"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "10000.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "100000.0"
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "NORTHERN AMERICA AREA"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'NAApprovalRule-ADC-1131-B-2',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "true",
				"IpmApprovalEnabled": "true",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsCyclone",
						"Operator": "Equal",
						"TargetValue": "false"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "10000.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "100000.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "CostType",
								"Operator": "Equal",
								"TargetValue": "Buyout"
							}
						]
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "NORTHERN AMERICA AREA"
					}
				]
			}'::jsonb,
			0
		)::t_rule ||
		(
			'NAApprovalRule-ADC-1131-C-1',
			rule_type,
			'{
				"CostConsultantIpmAllowed": "false",
				"IpmApprovalEnabled": "false",
				"BrandApprovalEnabled": "true"
			}'::jsonb,
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsCyclone",
						"Operator": "Equal",
						"TargetValue": "false"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "10000.0"
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "NORTHERN AMERICA AREA"
					}
				]
			}'::jsonb,
			0
		)::t_rule
	);
end 
$$ language plpgsql;