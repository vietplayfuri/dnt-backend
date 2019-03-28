do $$
declare 
	tmp_rules t_rule[] = array[]::t_rule[];
	rule_type int2;
begin
	-- Common payment rules
	rule_type := 0;
	perform fn_update_business_rules(
		rule_type, 
		tmp_rules ||
		(
			'ChinaPaymentRule1',
			rule_type,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThan",
						"TargetValue": "0.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "GREATER CHINA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "AAK (Asia)"
							}
						]
					}
				]
			}',
			0
		)::t_rule
	);

	-- NonAIPEPayment rules
	tmp_rules := array[]::t_rule[];
	rule_type := 3;

	perform fn_update_business_rules(
		rule_type,
		tmp_rules ||
		-- new - full production (AIPE=false; Region=E, NA, EMEA; Cost Type = Production; Production Type = Full Production; Budget > 0)
		(
			'FullProduction-NonAIPE-NonDPV',
			rule_type,
			'{
			"DetailedSplit": "true",
			"Splits": [
				{
				"CostTotalName": "production",
				"OESplit": "0.5",
				"FPSplit": "1.00",
				"FASplit": "1.00"
				},
				{
				"CostTotalName": "insuranceNotCovered",
				"OESplit": "1.00",
				"FPSplit": "1.00",
				"FASplit": "1.00"
				},
				{
				"CostTotalName": "technicalFee",
				"OESplit": "1.00",
				"FPSplit": "1.00",
				"FASplit": "1.00"
				},
				{
				"CostTotalName": "postProduction",
				"OESplit": "0.0",
				"FPSplit": "0.5",
				"FASplit": "1.00"
				},
				{
				"CostTotalName": "AllOtherCosts",
				"OESplit": "0.0",
				"FPSplit": "0.0",
				"FASplit": "1.00"
				}
			]
			}',
			'{
			"Operator": "And",
			"Children": [
				{
				"FieldName": "TotalCostAmount",
				"Operator": "GreaterThan",
				"TargetValue": "0.0"
				},
				{
				"FieldName": "ProductionType",
				"Operator": "Equal",
				"TargetValue": "Full Production"
				},
				{
				"FieldName": "IsAIPE",
				"Operator": "Equal",
				"TargetValue": "false"
				},
				{
				"Operator": "Or",
				"Children": [
					{
					"FieldName": "ContentType",
					"Operator": "Equal",
					"TargetValue": "Video"
					},
					{
					"FieldName": "ContentType",
					"Operator": "Equal",
					"TargetValue": "Photography"
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
					},
					{
					"FieldName": "BudgetRegion",
					"Operator": "Equal",
					"TargetValue": "AAK (Asia)"
					}
				]
				}
			]
			}',
			0
		)::t_rule ||

		-- ADC-2237 - full production (AIPE=false; Region=NA; Cost Type = Production; Production Type = Full Production)
		(
			'ADC-2237-FullProduction-VideoPhotography-NA',
			rule_type,
			'{
			"DetailedSplit": "true",
			"Splits": [
				{
				"CostTotalName": "production",
				"OESplit": "0.5",
				"FPSplit": "1.00",
				"FASplit": "1.00"
				},
				{
				"CostTotalName": "talentFees",
				"OESplit": "1.00",
				"FPSplit": "1.00",
				"FASplit": "1.00"
				},
				{
				"CostTotalName": "insuranceNotCovered",
				"OESplit": "1.00",
				"FPSplit": "1.00",
				"FASplit": "1.00"
				},
				{
				"CostTotalName": "technicalFee",
				"OESplit": "1.00",
				"FPSplit": "1.00",
				"FASplit": "1.00"
				},
				{
				"CostTotalName": "postProduction",
				"OESplit": "0.0",
				"FPSplit": "0.5",
				"FASplit": "1.00"
				},
				{
				"CostTotalName": "AllOtherCosts",
				"OESplit": "0.0",
				"FPSplit": "0.0",
				"FASplit": "1.00"
				}
			]
			}',
			'{
			"Operator": "And",
			"Children": [
				{
				"FieldName": "ProductionType",
				"Operator": "Equal",
				"TargetValue": "Full Production"
				},
				{
				"Operator": "Or",
				"Children": [
					{
					"FieldName": "ContentType",
					"Operator": "Equal",
					"TargetValue": "Video"
					},
					{
					"FieldName": "ContentType",
					"Operator": "Equal",
					"TargetValue": "Photography"
					}
				]
				},
				{
				"Operator": "Or",
				"Children": [
					{
					"FieldName": "BudgetRegion",
					"Operator": "Equal",
					"TargetValue": "NORTHERN AMERICA AREA"
					}
				]
				}
			]
			}',
			0
		)::t_rule ||

  		-- new - post production, CGI (AIPE=false; Region=E, NA, EMEA; Cost Type = Production; Production Type = Post Production, CGI; Budget >= 50k)
  		-- RULE A
		(
			'postProductionCGI-NonAIPE-NonDPV-A',
			rule_type,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"OESplit": "0.5",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "50000.0"
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "false"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "ProductionType",
								"Operator": "Equal",
								"TargetValue": "Post Production Only"
							},
							{
								"FieldName": "ProductionType",
								"Operator": "Equal",
								"TargetValue": "CGI/Animation"
							}
						]
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Video"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Photography"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Audio"
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
								"TargetValue": "NORTHERN AMERICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "INDIA & MIDDLE EAST AFRICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "AAK (Asia)"
							}
						]
					}
				]
			}',
			0
		)::t_rule ||

		-- new - post production, CGI (AIPE=false; Region=E, NA, EMEA; Cost Type = Production; Production Type = Post Production, CGI; Budget < 50k)
		-- RULE B
		(
			'postProductionCGI-NonAIPE-NonDPV-B',
			rule_type,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThan",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "50000.0"
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "false"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "ProductionType",
								"Operator": "Equal",
								"TargetValue": "Post Production Only"
							},
							{
								"FieldName": "ProductionType",
								"Operator": "Equal",
								"TargetValue": "CGI/Animation"
							}
						]
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Video"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Photography"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Audio"
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
								"TargetValue": "NORTHERN AMERICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "INDIA & MIDDLE EAST AFRICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "AAK (Asia)"
							}
						]
					}
				]
			}',
			0
		)::t_rule ||

		-- new - digital (AIPE=false; Region=E, NA, EMEA; Cost Type = Production; Content Type = Digital; Budget >= 50k)
  		-- RULE C
		(
			'Digital-NonAIPE-NonDPV-C',
			rule_type,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"OESplit": "0.5",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "50000.0"
					},
					{
						"FieldName": "ContentType",
						"Operator": "Equal",
						"TargetValue": "Digital"
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "false"
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
								"TargetValue": "NORTHERN AMERICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "INDIA & MIDDLE EAST AFRICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "AAK (Asia)"
							}
						]
					}
				]
			}',
			0
		)::t_rule ||

		-- new - digital (AIPE=false; Region=E, NA, EMEA; Cost Type = Production; Content Type = Digital; Budget < 50k)
  		-- RULE D
		(
			'Digital-NonAIPE-NonDPV-D',
			rule_type,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThan",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "50000.0"
					},
					{
						"FieldName": "ContentType",
						"Operator": "Equal",
						"TargetValue": "Digital"
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "false"
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
								"TargetValue": "NORTHERN AMERICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "INDIA & MIDDLE EAST AFRICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "AAK (Asia)"
							}
						]
					}
				]
			}',
			0
		)::t_rule ||

		-- new - Usage Buyout (AIPE=false; Region=ALL; Cost Type = Usage Buyout; Budget > 0)
		-- RULE F
		(
			'UsageBuyout-NonAIPE-NonDPV-F',
			rule_type,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThan",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "false"
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
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "JAPAN"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "GREATER CHINA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "LATIN AMERICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "EUROPE AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "NORTHERN AMERICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "INDIA & MIDDLE EAST AFRICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "AAK (Asia)"
							}
						]
					}
				]
			}',
			0
		)::t_rule ||
  		
		-- updated - other regions, latin america
  		-- RULE G
		(
			'LatinAmerica-NonAIPE-NonDPV-G',
			rule_type,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"OESplit": "0.5",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThan",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "false"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Video"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Photography"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Audio"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Digital"
							}
						]
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "LATIN AMERICA AREA"
					}
				]
			}',
			0
		)::t_rule ||

		-- updated - other regions, China
  		-- RULE H
		(
			'China-NonAIPE-NonDPV-H',
			rule_type,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThan",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "false"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Video"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Photography"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Audio"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Digital"
							}
						]
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "GREATER CHINA AREA"
					}
				]
			}',
			0
		)::t_rule ||
		
		-- updated - other regions, Japan
		-- RULE J
		(
			'Japan-NonAIPE-NonDPV-J',
			rule_type,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThan",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "false"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Video"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Photography"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Audio"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Digital"
							}
						]
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "JAPAN"
					}
				]
			}',
			0
		)::t_rule
	);

	-- AIPEPayment rules
	tmp_rules := array[]::t_rule[];
	rule_type := 4;

	perform fn_update_business_rules(
		rule_type,
		tmp_rules ||
		
		-- new - aipe - full production (AIPE=true; Region=E, NA, EMEA; Cost Type = Production; Production Type = Full Production; Budget > 0)
  		-- ADC-978
		(
			'FullProduction-AIPE-NonDPV-A',
			rule_type,
			'{
				"DetailedSplit": "true",
				"Splits": [
					{
						"CostTotalName": "TargetBudgetTotal",
						"AIPESplit": "0.5",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "0.0"
					},
					{
						"CostTotalName": "production",
						"AIPESplit": "0.0",
						"OESplit": "0.0",
						"FPSplit": "1.00",
						"FASplit": "1.00"
					},
					{
						"CostTotalName": "insuranceNotCovered",
						"AIPESplit": "0.0",
						"OESplit": "0.0",
						"FPSplit": "1.00",
						"FASplit": "1.00"
					},
					{
						"CostTotalName": "technicalFee",
						"AIPESplit": "0.0",
						"OESplit": "0.0",
						"FPSplit": "1.00",
						"FASplit": "1.00"
					},
					{
						"CostTotalName": "postProduction",
						"AIPESplit": "0.0",
						"OESplit": "0.0",
						"FPSplit": "0.5",
						"FASplit": "1.00"
					},
					{
						"CostTotalName": "AllOtherCosts",
						"AIPESplit": "0.0",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThan",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "ProductionType",
						"Operator": "Equal",
						"TargetValue": "Full Production"
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Video"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Photography"
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
								"TargetValue": "NORTHERN AMERICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "INDIA & MIDDLE EAST AFRICA AREA"
							}
						]
					}
				]
			}',
			0
		)::t_rule ||

		-- new - aipe - full production (AIPE=true; Region=E, NA, EMEA; Cost Type = Production; Production Type = Post Production, CGI/Animation, Content type = Video; Budget > 50k)
  		-- ADC-980
		(
			'FullProduction-AIPE-NonDPV-B',
			rule_type,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"AIPESplit": "0.5",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"FieldName": null,
				"Operator": "And",
				"TargetValue": null,
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "50000.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "ProductionType",
								"Operator": "Equal",
								"TargetValue": "Post Production Only"
							},
							{
								"FieldName": "ProductionType",
								"Operator": "Equal",
								"TargetValue": "CGI/Animation"
							}
						]
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Video"
							},
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Photography"
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
								"TargetValue": "NORTHERN AMERICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "INDIA & MIDDLE EAST AFRICA AREA"
							}
						]
					}
				]
			}',
			0
		)::t_rule ||
		
		-- new - aipe - audio (AIPE=true; Region=E, NA, EMEA; Cost Type = Production; Production Type = Post Production; Content type = Audio; Budget > 50k)
  		-- ADC-976
		(
			'FullProduction-AIPE-NonDPV-C',
			rule_type,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"AIPESplit": "0.5",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "50000.0"
					},
					{
						"FieldName": "ProductionType",
						"Operator": "Equal",
						"TargetValue": "Post Production Only"
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "ContentType",
						"Operator": "Equal",
						"TargetValue": "Audio"
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
								"TargetValue": "NORTHERN AMERICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "INDIA & MIDDLE EAST AFRICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "AAK (Asia)"
							}
						]
					}
				]
			}',
			0
		)::t_rule ||
		
		-- ADC-976 missing rule after refinement ADC-1306:(AIPE = true; Region = Europe; Cost Type = Production; Production Type = Full Production; Content type = Audio; Budget > 50k)
		(
			'FullProduction-AIPE-NonDPV-C-Europe',
			rule_type,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"AIPESplit": "0.5",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "50000.0"
					},
					{
						"FieldName": "ProductionType",
						"Operator": "Equal",
						"TargetValue": "Full Production"
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "ContentType",
						"Operator": "Equal",
						"TargetValue": "Audio"
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "EUROPE AREA"
					}
				]
			}',
			0
		)::t_rule ||

		-- new - aipe - AAK (CR-012) (AIPE=true; Region=AAK; Content Type = Video, Cost Type = Production; Production Type = Full Production; Budget > 0)
		(
			'AAK-FullProduction-AIPE-NonDPV-A',
			rule_type,
			'{
				"DetailedSplit": "true",
				"Splits": [
					{
						"CostTotalName": "TargetBudgetTotal",
						"AIPESplit": "0.5",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "0.0"
					},
					{
						"CostTotalName": "production",
						"AIPESplit": "0.0",
						"OESplit": "0.0",
						"FPSplit": "1.00",
						"FASplit": "1.00"
					},
					{
						"CostTotalName": "insuranceNotCovered",
						"AIPESplit": "0.0",
						"OESplit": "0.0",
						"FPSplit": "1.00",
						"FASplit": "1.00"
					},
					{
						"CostTotalName": "technicalFee",
						"AIPESplit": "0.0",
						"OESplit": "0.0",
						"FPSplit": "1.00",
						"FASplit": "1.00"
					},
					{
						"CostTotalName": "postProduction",
						"AIPESplit": "0.0",
						"OESplit": "0.0",
						"FPSplit": "0.5",
						"FASplit": "1.00"
					},
					{
						"CostTotalName": "AllOtherCosts",
						"AIPESplit": "0.0",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThan",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "ProductionType",
						"Operator": "Equal",
						"TargetValue": "Full Production"
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "ContentType",
						"Operator": "Equal",
						"TargetValue": "Video"
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "AAK (Asia)"
					}
				]
			}',
			0
		)::t_rule ||

  		-- new - aipe - AAK (CR-012) (AIPE=true; Region=AAK; Content Type = Video, Cost Type = Production; Production Type = Post Produciton, CGI; Budget > 50k)
		(
			'AAK-VideoPostProduction-AIPE-NonDPV-B',
			rule_type,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"AIPESplit": "0.5",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.0"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "50000.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "ProductionType",
								"Operator": "Equal",
								"TargetValue": "Post Production Only"
							},
							{
								"FieldName": "ProductionType",
								"Operator": "Equal",
								"TargetValue": "CGI/Animation"
							}
						]
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "ContentType",
						"Operator": "Equal",
						"TargetValue": "Video"
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "AAK (Asia)"
					}
				]
			}',
			0
		)::t_rule ||
		
		-- new - aipe - AAK (CR-012) (AIPE=true; Region=AAK; Content Type = Video, Cost Type = Production; Production Type = Post Produciton, CGI; Budget < 50k)
		(
			'AAK-VideoPostProduction-AIPE-NonDPV-C',
			rule_type,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"AIPESplit": "0.0",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThan",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "50000.0"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "ProductionType",
								"Operator": "Equal",
								"TargetValue": "Post Production Only"
							},
							{
								"FieldName": "ProductionType",
								"Operator": "Equal",
								"TargetValue": "CGI/Animation"
							}
						]
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "ContentType",
						"Operator": "Equal",
						"TargetValue": "Video"
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "AAK (Asia)"
					}
				]
			}',
			0
		)::t_rule ||
		
		-- aipe - AAK (AIPE=true; Region=AAK; Content Type = Photography, Cost Type = Production; Production Type = Full Production; Budget > 0)
		(
			'AAK-StillFullProduction-AIPE-NonDPV-D',
			rule_type,
			'{
				"DetailedSplit": "true",
				"Splits": [
					{
						"CostTotalName": "TargetBudgetTotal",
						"AIPESplit": "0.5",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "0.0"
					},
					{
						"CostTotalName": "production",
						"AIPESplit": "0.0",
						"OESplit": "0.0",
						"FPSplit": "1.00",
						"FASplit": "1.00"
					},
					{
						"CostTotalName": "insuranceNotCovered",
						"AIPESplit": "0.0",
						"OESplit": "0.0",
						"FPSplit": "1.00",
						"FASplit": "1.00"
					},
					{
						"CostTotalName": "technicalFee",
						"AIPESplit": "0.0",
						"OESplit": "0.0",
						"FPSplit": "1.00",
						"FASplit": "1.00"
					},
					{
						"CostTotalName": "postProduction",
						"AIPESplit": "0.0",
						"OESplit": "0.0",
						"FPSplit": "0.5",
						"FASplit": "1.00"
					},
					{
						"CostTotalName": "AllOtherCosts",
						"AIPESplit": "0.0",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThan",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "ProductionType",
						"Operator": "Equal",
						"TargetValue": "Full Production"
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "ContentType",
						"Operator": "Equal",
						"TargetValue": "Photography"
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "AAK (Asia)"
					}
				]
			}',
			0
		)::t_rule ||
		
		-- new - AAK (AIPE=true; Region=AAK; Content Type = Photography, Cost Type = Production; Production Type = Post Produciton; Budget > 50k)
		(
			'AAK-StillPostProduction-AIPE-NonDPV-E',
			rule_type,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"AIPESplit": "0.5",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "50000.0"
					},
					{
						"FieldName": "ProductionType",
						"Operator": "Equal",
						"TargetValue": "Post Production Only"
					},
					{
						"FieldName": "IsAIPE",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "ContentType",
						"Operator": "Equal",
						"TargetValue": "Photography"
					},
					{
						"FieldName": "BudgetRegion",
						"Operator": "Equal",
						"TargetValue": "AAK (Asia)"
					}
				]
			}',
			0
		)::t_rule ||
		(
			'DistributionPaymentRule',
			3,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "CostType",
						"Operator": "Equal",
						"TargetValue": "Trafficking"
					}
				]
			}',
			0
		)::t_rule ||
		
		-- Refinement - New Payment Rules ADC-1306 230, 233, 981
		(
			'FullProduction-NonAIPE-ADC-1306_230_233',
			3,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"OESplit": "0.5",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThanOrEqual",
						"TargetValue": "50000.0"
					},
					{
						"FieldName": "ProductionType",
						"Operator": "Equal",
						"TargetValue": "Full Production"
					},
					{
						"FieldName": "ContentType",
						"Operator": "Equal",
						"TargetValue": "Audio"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "NORTHERN AMERICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "INDIA & MIDDLE EAST AFRICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "EUROPE AREA"
							}
						]
					}
				]
			}',
			0
		)::t_rule ||
		
		-- Refinement - New Payment Rules ADC-1306 231,234,981
		(
			'FullProduction-NonAIPE-ADC-1306_231_234',
			3,
			'{
				"DetailedSplit": "false",
				"Splits": [
					{
						"CostTotalName": "CostTotal",
						"OESplit": "0.0",
						"FPSplit": "0.0",
						"FASplit": "1.00"
					}
				]
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "TotalCostAmount",
						"Operator": "GreaterThan",
						"TargetValue": "0.0"
					},
					{
						"FieldName": "TotalCostAmount",
						"Operator": "LessThan",
						"TargetValue": "50000.0"
					},
					{
						"FieldName": "ProductionType",
						"Operator": "Equal",
						"TargetValue": "Full Production"
					},
					{
						"FieldName": "ContentType",
						"Operator": "Equal",
						"TargetValue": "Audio"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "NORTHERN AMERICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "INDIA & MIDDLE EAST AFRICA AREA"
							},
							{
								"FieldName": "BudgetRegion",
								"Operator": "Equal",
								"TargetValue": "EUROPE AREA"
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