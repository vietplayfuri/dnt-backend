do $$
declare 
	tmp_rules t_rule[] = array[]::t_rule[];
	rule_type int2 = 5;
begin
	perform fn_update_business_rules(
		rule_type,
		tmp_rules ||
		
		-- This is common stage workflow. Other rules can overried/add additional stages for this workflow.
		(
			'CommonStages',
			rule_type,
			'{
				"Add": {
					"Stages": {
						"New": {
							"Name": "New",
							"IsRequired": "true"
						},
						"OriginalEstimate": {
							"Name": "Original Estimate",
							"IsRequired": "true",
							"IsCalculatingPayment": "true"
						},
						"OriginalEstimateRevision": {
							"Name": "Current Revision",
							"IsCalculatingPayment": "false"
						},
						"FirstPresentation": {
							"Name": "First Presentation",
							"IsRequired": "true",
							"IsCalculatingPayment": "true"
						},
						"FirstPresentationRevision": {
							"Name": "Current Revision",
							"IsCalculatingPayment": "false"
						},
						"FinalActual": {
							"IsRequired": "true",
							"Name": "Final Actual",
							"IsCalculatingPayment": "true"
						}
					},
					"Transitions": {
						"New": [
							"OriginalEstimate"
						],
						"OriginalEstimate": [
							"OriginalEstimateRevision",
							"FirstPresentation"
						],
						"OriginalEstimateRevision": [
							"FirstPresentation"
						],
						"FirstPresentation": [
							"FirstPresentationRevision",
							"FinalActual"
						],
						"FirstPresentationRevision": [
							"FinalActual"
						]
					}
				}
			}',
			null,
			100
		)::t_rule ||
		
		-- Is Aipe step applicable?
        -- RULE FOR ADC-574
        -- RULE FOR ADC-575
        -- RULE FOR ADC-576
		(
			'Aipe',
			rule_type,
			'{
				"Add": {
					"Stages": {
						"Aipe": {
							"Name": "Aipe",
							"IsRequired": "false",
							"IsCalculatingPayment": "true"
						}
					},
					"Transitions": {
						"New": [
							"Aipe"
						],
						"Aipe": [
							"OriginalEstimate"
						]
					}
				}
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
			200
		)::t_rule ||
  		
		-- Skip first presentation rule
		(
			'SkipFirstPresentation',
			rule_type,
			'{
				"Add": {
					"Transitions": {
						"New": [
							"FinalActual"
						],
						"OriginalEstimate": [
							"FinalActual"
						],
						"OriginalEstimateRevision": [
							"FinalActual"
						]
					}
				},
				"Remove": {
					"Stages": [
						"FirstPresentation",
						"FirstPresentationRevision"
					],
					"Transitions": {}
				}
			}',
			'{
				"FieldName": null,
				"Operator": "Or",
				"TargetValue": null,
				"Children": [
					{
						"FieldName": "CostType",
						"Operator": "Equal",
						"TargetValue": "Buyout",
						"Children": []
					},
					{
						"FieldName": "CostType",
						"Operator": "Equal",
						"TargetValue": "Trafficking",
						"Children": []
					},
					{
						"FieldName": "ContentType",
						"Operator": "Equal",
						"TargetValue": "Digital",
						"Children": []
					},
					{
						"FieldName": "ContentType",
						"Operator": "Equal",
						"TargetValue": "Audio",
						"Children": []
					},
					{
						"FieldName": null,
						"Operator": "And",
						"TargetValue": null,
						"Children": [
							{
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Photography",
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
										"FieldName": null,
										"Operator": "And",
										"TargetValue": null,
										"Children": [
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
														"FieldName": "BudgetRegion",
														"Operator": "Equal",
														"TargetValue": "LATIN AMERICA AREA",
														"Children": []
													},
													{
														"FieldName": "BudgetRegion",
														"Operator": "Equal",
														"TargetValue": "JAPAN",
														"Children": []
													},
													{
														"FieldName": "BudgetRegion",
														"Operator": "Equal",
														"TargetValue": "GREATER CHINA AREA",
														"Children": []
													}
												]
											}
										]
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
								"FieldName": "ContentType",
								"Operator": "Equal",
								"TargetValue": "Video",
								"Children": []
							},
							{
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
												"FieldName": "ProductionType",
												"Operator": "Equal",
												"TargetValue": "CGI/Animation",
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
														"FieldName": "BudgetRegion",
														"Operator": "Equal",
														"TargetValue": "LATIN AMERICA AREA",
														"Children": []
													},
													{
														"FieldName": "BudgetRegion",
														"Operator": "Equal",
														"TargetValue": "JAPAN",
														"Children": []
													},
													{
														"FieldName": "BudgetRegion",
														"Operator": "Equal",
														"TargetValue": "GREATER CHINA AREA",
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
												"FieldName": "ProductionType",
												"Operator": "Equal",
												"TargetValue": "Post Production Only",
												"Children": []
											}
										]
									}
								]
							}
						]
					}
				]
			}',
			300
		)::t_rule
	);
end 
$$ language plpgsql;	