do $$
declare 
	tmp_rules t_rule [] = array [] :: t_rule [];
	rule_type int2 = 7;
begin 
	perform fn_update_business_rules(
		rule_type,
		tmp_rules || (
        'Submit',
        rule_type,
        '{
				"Actions": [
					"Submit"
				]
			}',
        '{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsOwner",
						"Operator": "Equal",
						"TargetValue": "true"
					},					
					{
						"FieldName": "IsLatestRevision",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "Status",
						"Operator": "Equal",
						"TargetValue": "Draft"
					}
				]
			}',
        0
    ) :: t_rule || (
        'MoveToNextStage',
        rule_type,
        '{
				"Actions": [
					"NextStage"
				]
			}',
        '{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsOwner",
						"Operator": "Equal",
						"TargetValue": "true"
					},					
					{
						"FieldName": "IsLatestRevision",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "Status",
						"Operator": "Equal",
						"TargetValue": "Approved"
					},
					{
						"FieldName": "CostStage",
						"Operator": "NotEqual",
						"TargetValue": "FinalActual"
					}
				]
			}',
        0
    ) :: t_rule || (
        'CreateRevision',
        rule_type,
        '{
				"Actions": [
					"CreateRevision"
				]
			}',
        '{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsOwner",
						"Operator": "Equal",
						"TargetValue": "true"
					},					
					{
						"FieldName": "IsLatestRevision",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "Status",
						"Operator": "Equal",
						"TargetValue": "Approved"
					},
					{
						"FieldName": "CostStage",
						"Operator": "NotEqual",
						"TargetValue": "FinalActual"
					},
					{
						"FieldName": "CostStage",
						"Operator": "NotEqual",
						"TargetValue": "Aipe"
					}
				]
			}',
        0
    ) :: t_rule || (
        'Reopen',
        rule_type,
        '{
				"Actions": [
					"Reopen"
				]
			}',
        '{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsOwner",
						"Operator": "Equal",
						"TargetValue": "true"
					},					
					{
						"FieldName": "IsLatestRevision",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "Status",
								"Operator": "Equal",
								"TargetValue": "Rejected"
							},
							{
								"FieldName": "Status",
								"Operator": "Equal",
								"TargetValue": "Recalled"
							}
						]
					}
				]
			}',
        0
    ) :: t_rule || (
        'ReopenApproveReject',
        rule_type,
        '{
				"Actions": [
					"ApproveReopen",
					"RejectReopen"
				]
			}',
        '{
				"Operator": "And",
				"Children":
				 [									
					{
						"FieldName": "IsLatestRevision",
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
								"FieldName": "IsAdmin",
								"Operator": "Equal",
								"TargetValue": "true"
							},
							{
								"FieldName": "Status",
								"Operator": "Equal",
								"TargetValue": "PendingReopen"
							},
							{
								"Operator": "Or",
								"Children": [
									{
										"FieldName": "CostStage",
										"Operator": "Equal",
										"TargetValue": "FinalActual"
									},
									{
										"FieldName": "CostStage",
										"Operator": "Equal",
										"TargetValue": "FinalActualRevision"
									}
								]
							}
						]
					}
				]
			}		
			]
			} 
			',
        0
    ) :: t_rule || -- ADC-546 
	(
        'Recall',
        rule_type,
        '{
				"Actions": [
					"Recall"
				]
			}',
        '{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsOwner",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "IsLatestRevision",
						"Operator": "Equal",
						"TargetValue": "true"
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
										"FieldName": "Status",
										"Operator": "Equal",
										"TargetValue": "PendingBrandApproval"
									},
									{
										"FieldName": "HasExternalIntegration",
										"Operator": "Equal",
										"TargetValue": "false"
									}
								]
							}
						]
					}
				]
			}',
        0
    ) :: t_rule || -- ADC-416
	 (
        'Cancel',
        rule_type,
        '{
				"Actions": [
					"Cancel"
				]
			}',
        '{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsOwner",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "IsLatestRevision",
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
										"FieldName": "Status",
										"Operator": "Equal",
										"TargetValue": "Approved"
									},
									{
										"FieldName": "CostStage",
										"Operator": "NotEqual",
										"TargetValue": "FinalActual"
									}
								]
							},
							{
								"FieldName": null,
								"Operator": "And",
								"TargetValue": null,
								"Children": [
									{
										"FieldName": "Status",
										"Operator": "Equal",
										"TargetValue": "Draft"
									},
									{
										"FieldName": "HasPONumber",
										"Operator": "Equal",
										"TargetValue": "true"
									},
									{
										"FieldName": "HasExternalIntegration",
										"Operator": "Equal",
										"TargetValue": "true"
									}
								]
							},
							{
								"Operator": "And",
								"Children": [
									{
										"FieldName": "Status",
										"Operator": "Equal",
										"TargetValue": "Draft"
									},
									{
										"FieldName": "HasExternalIntegration",
										"Operator": "Equal",
										"TargetValue": "false"
									}
								]
							},
							{
								"Operator": "And",
								"Children": [
									{
										"FieldName": "Status",
										"Operator": "Equal",
										"TargetValue": "PendingBrandApproval"
									},
									{
										"FieldName": "HasPONumber",
										"Operator": "Equal",
										"TargetValue": "true"
									},
									{
										"FieldName": "HasExternalIntegration",
										"Operator": "Equal",
										"TargetValue": "true"
									}
								]
							}
						]
					}
				]
			}',
        0
    ) :: t_rule || (
        'RequestReopen',
        rule_type,
        '{
				"Actions": [
					"RequestReopen"
				]
			}',
        '{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsOwner",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "IsLatestRevision",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "Status",
						"Operator": "Equal",
						"TargetValue": "Approved"
					},
					{
						"FieldName": "CostStage",
						"Operator": "Equal",
						"TargetValue": "FinalActual"
					}
				]
			}',
        0
    ) :: t_rule || -- ADC-548
	 (
        'Delete',
        rule_type,
        '{
				"Actions": [
					"Delete"
				]
			}',
        '{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "IsOwner",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "IsLatestRevision",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "Status",
						"Operator": "Equal",
						"TargetValue": "Draft"
					},
					{
						"FieldName": "NeverSubmitted",
						"Operator": "Equal",
						"TargetValue": "true"
					}
				]
			}',
        0
    ) :: t_rule || (
        'Approve',
        rule_type,
        '{
				"Actions": [
					"Approve"
				]
			}',
        '
		{
			"Operator": "And",
			"Children": [
    			{
					"FieldName": "IsLatestRevision",
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
									"FieldName": "IsApprover",
									"Operator": "Equal",
									"TargetValue": "true"
            					},
            					{
									"FieldName": "Status",
									"Operator": "Equal",
									"TargetValue": "PendingTechnicalApproval"
            					},
            					{
									"FieldName": "CostTotalBelowAuthLimit",
									"Operator": "Equal",
									"TargetValue": "true"
            					}
          					]
        				},
       				 	{
							"Operator": "And",
							"Children": [
            					{
									"FieldName": "IsApprover",
									"Operator": "Equal",
									"TargetValue": "true"
            					},
            					{
									"FieldName": "Status",
									"Operator": "Equal",
									"TargetValue": "PendingBrandApproval"
            					},
           						{
									"FieldName": "HasExternalIntegration",
									"Operator": "Equal",
									"TargetValue": "false"
            					},
           						{
									"FieldName": "CostStageTotal",
									"Operator": "GreaterThanOrEqual",
									"TargetValue": "0"
            					},
           						{
									"FieldName": "CostTotalBelowAuthLimit",
									"Operator": "Equal",
									"TargetValue": "true"
            					}
         					 ]
        				}
     				 ]
    			}					
  			]
		}
		',
        0
    ) :: t_rule || (
        'Reject',
        rule_type,
        '{
				"Actions": [
					"Reject"
				]
			}',
        '{
			"Operator": "And",
			"Children": [
    			{
					"FieldName": "IsLatestRevision",
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
									"FieldName": "IsApprover",
									"Operator": "Equal",
									"TargetValue": "true"
            					},
								{
								"FieldName": "Status",
								"Operator": "Equal",
								"TargetValue": "PendingTechnicalApproval"
								}
          					]
        				},
        				{
							"Operator": "And",
							"Children": [
            					{
									"FieldName": "IsApprover",
									"Operator": "Equal",
									"TargetValue": "true"
            					},
            					{
									"FieldName": "Status",
									"Operator": "Equal",
									"TargetValue": "PendingBrandApproval"
            					},
           						{
									"FieldName": "HasExternalIntegration",
									"Operator": "Equal",
									"TargetValue": "false"
            					}
          					]
       					 }
      				]
   				}
  			]
		}',
        0
    ) :: t_rule || (
        'EditValueReporting',
        rule_type,
        '{
				"Actions": [
					"EditValueReporting"
				]
			}',
        '{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "CostStage",
						"Operator": "NotEqual",
						"TargetValue": "Aipe"
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"Operator": "And",
								"Children": [
									{
										"FieldName": "UserIsIPMAndApproved",
										"Operator": "Equal",
										"TargetValue": "true"
									},
									{
										"FieldName": "IsLatestRevision",
										"Operator": "Equal",
										"TargetValue": "true"
									}
								]
							},
							{
								"FieldName": "IsAdmin",
								"Operator": "Equal",
								"TargetValue": "true"
							}
						]
					},
					{
						"Operator": "Or",
						"Children": [
							{
								"FieldName": "Status",
								"Operator": "Equal",
								"TargetValue": "PendingBrandApproval"
							},
							{
								"FieldName": "Status",
								"Operator": "Equal",
								"TargetValue": "Approved"
							}
						]
					}
				]
			}',
        0
    ) :: t_rule || (
        'EditIONumber',
        rule_type,
        '{
				"Actions": [
					"EditIONumber"
				]
			}',
        '{
				"Operator": "And",
				"Children": [
					{
						"FieldName": "UserIsFinanceManager",
						"Operator": "Equal",
						"TargetValue": "true"
					},
					{
						"FieldName": "Status",
						"Operator": "Equal",
						"TargetValue": "Approved"
					},
					{
						"FieldName": "HasExternalIntegration",
						"Operator": "Equal",
						"TargetValue": "false"
					}
				]
			}',
        0
    ) :: t_rule
);
end 
$$ language plpgsql;