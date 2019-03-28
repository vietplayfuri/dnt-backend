do $$
declare 
	tmp_rules t_rule[] = array[]::t_rule[];
	rule_type int2 = 8;
begin
	perform fn_update_business_rules(
		rule_type,
		tmp_rules ||
		(
			'Transitions_To_Draft',
			rule_type,
			'{
				"Status": "Draft"
			}',
			'{
				"Operator": "Or",
				"Children": [
					{
						"FieldName": "Action",
						"Operator": "Equal",
						"TargetValue": "NextStage"
					},
					{
						"FieldName": "Action",
						"Operator": "Equal",
						"TargetValue": "CreateRevision"
					},
					{
						"FieldName": "Action",
						"Operator": "Equal",
						"TargetValue": "Reopen"
					}
				]
			}',
			0
		)::t_rule ||
  		
		-- ADC-1988 reopen FinalActual 
		(
			'Transitions_To_Pending_Reopen',
			rule_type,
			'{
				"Status": "PendingReopen"
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "Action",
						"Operator": "Equal",
						"TargetValue": "RequestReopen"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "Status",
								"Operator": "Equal",
								"TargetValue": "Approved"
							},
							{
								"FieldName": "Status",
								"Operator": "Equal",
								"TargetValue": "Draft"
							}
						]
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "CostStage",
								"Operator": "Equal",
								"TargetValue": "Final Actual"
							},
							{
								"FieldName": "CostStage",
								"Operator": "Equal",
								"TargetValue": "FinalActual"
							}
						]
					}
				]
			}',
			0
		)::t_rule ||
		(
			'Transitions_To_PendingTechnicalApproval',
			rule_type,
			'{
				"Status": "PendingTechnicalApproval"
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "Action",
						"Operator": "Equal",
						"TargetValue": "Submit"
					},
					{
						"FieldName": "Status",
						"Operator": "Equal",
						"TargetValue": "Draft"
					},
					{
						"FieldName": "HasTechnicalApproval",
						"Operator": "Equal",
						"TargetValue": "true"
					}
				]
			}',
			0
		)::t_rule ||
		(
			'Transitions_To_PendingBrandApproval',
			rule_type,
			'{
				"Status": "PendingBrandApproval"
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "HasBrandApproval",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"Operator": "And",
								"Children": [
									{
										"FieldName": "Action",
										"Operator": "Equal",
										"TargetValue": "Submit"
									},
									{
										"FieldName": "Status",
										"Operator": "Equal",
										"TargetValue": "Draft"
									},
									{
										"FieldName": "HasTechnicalApproval",
										"Operator": "Equal",
										"TargetValue": "false"
									}
								]
							},
							{
								"Operator": "And",
								"Children": [
									{
										"FieldName": "Action",
										"Operator": "Equal",
										"TargetValue": "Approve"
									},
									{
										"FieldName": "Status",
										"Operator": "Equal",
										"TargetValue": "PendingTechnicalApproval"
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
			'Transitions_To_Approval',
			rule_type,
			'{
				"Status": "Approved"
			}',
			'{
				"Operator": "Or",
				"Children": [
					{
						"Operator": "And",
						"Children": [
							{
								"FieldName": "HasBrandApproval",
								"Operator": "Equal",
								"TargetValue": "true"
							},
							{
								"FieldName": "Status",
								"Operator": "Equal",
								"TargetValue": "PendingBrandApproval"
							},
							{
								"FieldName": "Action",
								"Operator": "Equal",
								"TargetValue": "Approve"
							}
						]
					},
					{
						"Operator": "And",
						"Children": [
							{
								"FieldName": "HasBrandApproval",
								"Operator": "Equal",
								"TargetValue": "false"
							},
							{
								"FieldName": "Status",
								"Operator": "Equal",
								"TargetValue": "PendingTechnicalApproval"
							},
							{
								"FieldName": "Action",
								"Operator": "Equal",
								"TargetValue": "Approve"
							}
						]
					},
					{
						"Operator": "And",
						"Children": [
							{
								"FieldName": "HasBrandApproval",
								"Operator": "Equal",
								"TargetValue": "false"
							},
							{
								"FieldName": "HasTechnicalApproval",
								"Operator": "Equal",
								"TargetValue": "false"
							},
							{
								"FieldName": "Action",
								"Operator": "Equal",
								"TargetValue": "Submit"
							}
						]
					}
				]
			}',
			0
		)::t_rule ||
		(
			'Recall_When_PendingTechnicalApproval',
			rule_type,
			'{
				"Status": "Recalled"
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "Action",
						"Operator": "Equal",
						"TargetValue": "Recall"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "Status",
								"Operator": "Equal",
								"TargetValue": "PendingTechnicalApproval"
							},
							{
								"Operator": "And",
								"Children": [
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
							}
						]
					}
				]
			}',
			0
		)::t_rule ||
		(
			'Cancell_When_HasExternalIntegration',
			rule_type,
			'{
				"Status": "PendingCancellation"
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "Action",
						"Operator": "Equal",
						"TargetValue": "Cancel"
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
			}',
			0
		)::t_rule ||
		(
			'Cancel_When_NoExternalIntegration',
			rule_type,
			'{
				"Status": "Cancelled"
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "Action",
						"Operator": "Equal",
						"TargetValue": "Cancel"
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
			}',
			0
		)::t_rule ||
		(
			'Reject_Always_MovesToRejectedStatus',
			rule_type,
			'{
				"Status": "Rejected"
			}',
			'{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "Action",
						"Operator": "Equal",
						"TargetValue": "Reject"
					}
				]
			}',
			0
		)::t_rule
	);
end 
$$ language plpgsql;	