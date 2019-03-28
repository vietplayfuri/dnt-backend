
CREATE TYPE cost_template_type AS (
	"name" text,
	label text,
	fields json
);

CREATE TYPE cli_item_type AS (
	"name" text,
	label text,
	read_only bool,
	mandatory bool,
	allow_negative bool,
	"order" int
);

CREATE TYPE cli_section_type AS (
	"name" text,
	label text,
	currency_label text,
	sub_total_label text,
	"order" int,
	items cli_item_type[]
);

CREATE TYPE form_section_definition_type AS (
	"name" text,
	label text,
	fields json
);

CREATE TYPE form_definition_type AS (
	"name" text,
	label text,
	productionType text,
	fields json,
	cli_sections cli_section_type[],
	form_sections form_section_definition_type[]
);

CREATE TYPE production_detail_template_type AS (
	"type" text,
	form_definitions form_definition_type[]
);

CREATE OR REPLACE FUNCTION fn_add_form_definition(_form_definition form_definition_type, _form_definition_id uuid, _cost_template_version_id uuid, _pdt_id uuid, _admin_user_id uuid) RETURNS void AS $$
DECLARE
	_fd_id uuid;
	_fsdt_id uuid;
	_cli_section_id uuid;
	_fsdt form_section_definition_type;
	_cli_section cli_section_type;
	_cli_item cli_item_type;
	_created timestamp := now();
	_modified timestamp := now();
begin
	_fd_id := uuid_generate_v1();
			
	--add production details forms			
	insert into custom_form_data (id, "data")
	values(_fd_id, _form_definition.fields::json);

	insert into form_definition(id, production_details_template_id, field_definitions_id, "name", label, cost_template_version_id, production_type)
	values(_form_definition_id, _pdt_id, _fd_id, _form_definition."name", _form_definition.label, _cost_template_version_id, _form_definition.productionType);
			
	RAISE NOTICE 'Inserted FDT: %', _form_definition."name";
			
	--add cost line item sections
	IF _form_definition.cli_sections IS NOT NULL THEN
		FOREACH _cli_section IN ARRAY _form_definition.cli_sections
		LOOP
			_cli_section_id := uuid_generate_v1();
			
			insert into cost_line_item_section_template(id, form_definition_id, "name", label, currency_label, sub_total_label, "order", created_by_id, created, modified)
			values (_cli_section_id, _form_definition_id, _cli_section."name", _cli_section.label, _cli_section.currency_label, _cli_section.sub_total_label, _cli_section."order", _admin_user_id, _created, _modified);
							
			RAISE NOTICE 'Inserted CLI Section: %', _cli_section."name";
			
			--add cost line items
			FOREACH _cli_item IN ARRAY _cli_section.items
			LOOP
				--add cost line items
				insert into cost_line_item_section_template_item(section_template_id, "name", label, read_only, mandatory, allow_negative, "order")
				values (_cli_section_id, _cli_item."name", _cli_item.label, _cli_item.read_only, _cli_item.mandatory, _cli_item.allow_negative, _cli_item."order");
								
				RAISE NOTICE 'Inserted CLI: %', _cli_item."label";					
			END LOOP;
		END LOOP;
	END IF;
	
	IF _form_definition.form_sections IS NOT NULL THEN
		
		FOREACH _fsdt IN ARRAY _form_definition.form_sections
		LOOP
			_fsdt_id := uuid_generate_v1();
			_fd_id := uuid_generate_v1();
			
			insert into custom_form_data (id, "data")
			values(_fd_id, _fsdt.fields::json);
	
			insert into form_section_definition(id, form_definition_id, field_definitions_id, "name", label)
			values(_fsdt_id, _form_definition_id, _fd_id, _fsdt."name", _fsdt.label);
			
			RAISE NOTICE 'Inserted FSDT: %', _fsdt."name";
		END LOOP;
	END IF;
		
END
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_add_cost_template_version(cost_type int, cost_template cost_template_type, production_detail_templates production_detail_template_type[], additional_forms form_definition_type[]) RETURNS void AS $$
DECLARE
	_abstract_type_id uuid;
	_admin_user_id uuid;
	_cost_template_field_definitions_id uuid := uuid_generate_v1();
	_cost_template_id uuid := uuid_generate_v1();
	_cost_template_version_id uuid := uuid_generate_v1();
	_cost_template_version_name text;
	_pdt_id uuid;
	_fdt_id uuid;
	_fd_id uuid;
	_fsdt_id uuid;
	_cli_section_id uuid;
	_pdt production_detail_template_type;
	_fdt form_definition_type;
	_fsdt form_section_definition_type;
	_cli_section cli_section_type;
	_cli_item cli_item_type;
	_created timestamp := now();
	_modified timestamp := now();
begin
	
	-- get the module abstract type id
	select id into _abstract_type_id from abstract_type a where type = 'Module' and a.id = a.parent_id;
	-- get the admin user id
	select id into _admin_user_id from cost_user cu where cu.email = 'costs.admin@adstream.com';
	
	-- create fields for the cost_template i.e. the Cost Details Screen
	insert into custom_form_data (id, "data")
	values(_cost_template_field_definitions_id, cost_template.fields::json);
	
	-- create a cost_template
	insert into cost_template (id, "name", label, abstract_type_id, cost_type, field_definitions_id, created_by_id, created, modified)
	values(_cost_template_id, cost_template."name", cost_template.label, _abstract_type_id, cost_type, _cost_template_field_definitions_id, _admin_user_id, _created, _modified);
	
	if cost_type = 0 then
		_cost_template_version_name := 'Production Cost';
	elsif cost_type = 1 then
		_cost_template_version_name := 'Buyout Cost';
	elsif cost_type = 2 then
		_cost_template_version_name := 'Trafficking/Distribution Cost';
	else
		raise exception 'Unsupported cost_type: %', cost_type;
	end if;
	
	-- create a cost_template_version
	insert into cost_template_version(id, cost_template_id, "name", owner_id, created_by_id, created, modified)
	values(_cost_template_version_id, _cost_template_id, _cost_template_version_name, _admin_user_id, _admin_user_id, _created, _modified);
		
	-- add production detail templates	
	FOREACH _pdt IN ARRAY production_detail_templates
	LOOP
		_pdt_id := uuid_generate_v1();
		insert into production_details_template(id, cost_template_version_id, "type")
			values(_pdt_id, _cost_template_version_id, _pdt."type");
		RAISE NOTICE 'Inserted PDT: %', _pdt."type";
		FOREACH _fdt IN ARRAY _pdt.form_definitions
		LOOP
			_fdt_id := uuid_generate_v1();
			-- cost_template_version_id needs to be null so that the cost backend entity model creates the correct model for the frontend to use.
			perform fn_add_form_definition(_fdt, _fdt_id, null, _pdt_id, _admin_user_id);			
		END LOOP;
	END LOOP;
	
	-- add additional forms (mainly for usage and buyout)
	IF additional_forms IS NOT NULL then
		FOREACH _fdt IN ARRAY additional_forms
		LOOP		
			_fdt_id := uuid_generate_v1();
			-- production_details_id needs to be null for usage/buyout costs so that the cost backend entity model creates the correct model for the frontend to use.
			perform fn_add_form_definition(_fdt, _fdt_id, _cost_template_version_id, null, _admin_user_id);			
		END LOOP;
	END IF;		
END
$$ LANGUAGE plpgsql;

-- Production Cost
select fn_add_cost_template_version(0, 
('costDetails', 'Cost Details', 
'[
    {
        "label": "Agency Name",
        "name": "agencyName",
        "type": "string"
    },
    {
        "label": "Agency Location",
        "name": "agencyLocation",
        "type": "string"
    },
    {
        "label": "Agency producer/art buyer",
        "name": "agencyProducerArtBuyer",
        "type": "string"
    },
    {
        "label": "Budget Region",
        "name": "budgetRegion",
        "type": "string"
    },
    {
        "label": "Campaign",
        "name": "brand",
        "type": "string"
    },
    {
        "label": "Content Type",
        "name": "contentType",
        "type": "string"
    },
    {
        "label": "Production Type",
        "name": "productionType",
        "type": "string"
    },
    {
        "label": "Target Budget",
        "name": "targetBudget",
        "type": "number",
        "mandatory": true
    },
    {
        "label": "Agency Tracking Number",
        "name": "contentType",
        "type": "string"
    },
    {
        "label": "Organisation",
        "name": "organisation",
        "type": "string"
    },
    {
        "label": "Agency Payment Currency",
        "name": "agencyCurrency",
        "type": "string"
    }
]'::json) ::cost_template_type, 
	ARRAY[('video', 
		ARRAY[
			('fullProductionWithShoot', 'Full Production', 'Full Production',
	        	'[{
	                "label": "Shoot Date",
	                "name": "shootDate",
	                "type": "string",
	                "mandatory": true
	            },
	            {
	                "label": "Production Company",
	                "name": "productionCompany",
	                "type": "string",
	                "mandatory": true
	            },
	            {
	                "label": "Director''s name",
	                "name": "director",
	                "type": "string",
	                "mandatory": true
	            },
	            {
	                "label": "Number of shoot days",
	                "name": "noShootDays",
	                "type": "number",
	                "mandatory": true
	            },
	            {
	                "label": "Shoot country",
	                "name": "primaryShootCountry",
	                "type": "string",
	                "mandatory": true
	            },
	            {
	                "label": "Shoot city",
	                "name": "primaryShootCity",
	                "type": "string",
	                "mandatory": true
	            },
	            {
	                "label": "Other Shoot country",
	                "name": "otherShootCountry",
	                "type": "string",
	                "mandatory": false
	            },
	            {
	                "label": "Other Shoot city",
	                "name": "otherShootCity",
	                "type": "string",
	                "mandatory": false
	            },
	            {
	                "label": "Post production company",
	                "name": "postProductionCompany",
	                "type": "string",
	                "mandatory": true
	            },
	            {
	                "label": "Music company",
	                "name": "musicCompany",
	                "type": "string",
	                "mandatory": [
	                    "FinalActual"
	                ]
	            },
	            {
	                "label": "CGI/Animation company",
	                "name": "cgiAnimationCompany",
	                "type": "string",
	                "mandatory": false
	            },
	            {
	                "label": "Talent company",
	                "name": "talentCompany",
	                "type": "string",
	                "mandatory": [
	                    "FinalActual"
	                ]
	            },
	            {
	                "label": "Direct Billing",
	                "name": "directBilling",
	                "type": "checkbox",
	                "mandatory": true
	            }]'::json,
	            ARRAY[
		            ('production', 'Production Cost', 'Production Currency', null, 0,
		            	ARRAY[
		            	('preProduction', 'Pre production/casting', false, true, false, 0)::cli_item_type,
		            	('talentFees', 'Talent fees', false, true, false, 10)::cli_item_type,
		            	('directorsFees', 'Director fees', false, true, false, 20)::cli_item_type,
		            	('crew', 'Crew salaries', false, true, false, 30)::cli_item_type,
		            	('equipment', 'Equipment', false, true, false, 40)::cli_item_type,
		            	('transportcatering', 'Transport/catering', false, true, false, 50)::cli_item_type,
		            	('artDepartment', 'Art department', false, true, false, 60)::cli_item_type,
		            	('travel', 'Travel (exc. Agency travel)', false, true, false, 70)::cli_item_type,
		            	('productionInsuranceNotCovered', 'Insurance (if not covered by P&G)', false, true, false, 80)::cli_item_type,
		            	('directorscut', 'Directors cut (if managed by PH)', false, true, false, 90)::cli_item_type,
		            	('phmarkup', 'PH mark up', false, true, false, 100)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
	            	('postProduction', 'Post Production Cost', 'Post Production Currency', null, 10,
		            	ARRAY[
		            	('offlineEdits', 'Offline edits', false, true, false, 0)::cli_item_type,
		            	('audioFinalization', 'Audio finalization', false, true, false, 10)::cli_item_type,
		            	('onlineVideoFinalization', 'Online video finalization', false, true, false, 20)::cli_item_type,
		            	('cgiAnimation', 'CGI/animation', false, true, false, 30)::cli_item_type,
		            	('pphmarkup', 'PPH mark up (if applicable)', false, true, false, 40)::cli_item_type		            	
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
	            	('agencyCosts', 'Agency Costs', null, null, 20,
		            	ARRAY[
		            	('agencyArtWorkPacks', 'Agency Artwork/packs', false, true, false, 0)::cli_item_type,
		            	('agencyTravel', 'Agency travel', false, true, false, 10)::cli_item_type,
		            	('music', 'Music', false, true, false, 20)::cli_item_type,
		            	('casting', 'Casting', false, true, false, 30)::cli_item_type,
		            	('insurance', 'Insurance', false, true, false, 40)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
	            	('otherCosts', 'Other Costs', null, null, 30,
		            	ARRAY[
		            	('taxImportation', 'Tax (if applicable)', false, true, false, 0)::cli_item_type,
		            	('PandGInsurance', 'P&G insurance', false, true, false, 10)::cli_item_type,
		            	('technicalFee', 'Technical fee (if applicable)', true, true, false, 20)::cli_item_type,
		            	('foreignExchange', 'FX (Loss) & Gain', false, true, true, 30)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type
	            ] ::cli_section_type[],
            	null
	            ) ::form_definition_type,
	            ('postProductionOnlyWithShoot', 'Post Production Only', 'Post Production Only',
	            '[
				    {
				        "label": "Post production company",
				        "name": "postProductionCompany",
				        "type": "string",
				        "mandatory": true
				    },
				    {
				        "label": "Music company",
				        "name": "musicCompany",
				        "type": "string",
				        "mandatory": false
				    },
				    {
				        "label": "CGI/Animation company",
				        "name": "cgiAnimationCompany",
				        "type": "string",
				        "mandatory": false
				    },
				    {
				        "label": "Direct Billing",
				        "name": "directBilling",
				        "type": "checkbox",
				        "mandatory": true
				    }
				]'::json, ARRAY[
	            	('postProduction', 'Post Production Cost', 'Post Production Currency', null, 10,
		            	ARRAY[
		            	('offlineEdits', 'Offline edits', false, true, false, 0)::cli_item_type,
		            	('audioFinalization', 'Audio finalization', false, true, false, 10)::cli_item_type,
		            	('onlineVideoFinalization', 'Online video finalization', false, true, false, 20)::cli_item_type,
		            	('cgiAnimation', 'CGI/animation', false, true, false, 30)::cli_item_type,
		            	('pphMarkup', 'PPH mark up (if applicable)', false, true, false, 40)::cli_item_type		            	
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
	            	('agencyCosts', 'Agency Costs', null, null, 20,
		            	ARRAY[
		            	('agencyArtWorkPacks', 'Agency artwork/packs', false, true, false, 0)::cli_item_type,
		            	('agencyTravel', 'Agency travel', false, true, false, 10)::cli_item_type,
		            	('insurance', 'Insurance (if not covered by P&G)', false, true, false, 20)::cli_item_type,
		            	('music', 'Music', false, true, false, 30)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
	            	('Other', 'Other Costs', null, null, 30,
		            	ARRAY[
		            	('taxImportation', 'Tax (if applicable)', false, true, false, 0)::cli_item_type,
		            	('pandGInsurance', 'P&G insurance', false, true, false, 10)::cli_item_type,
		            	('technicalFee', 'Technical fee (if applicable)', true, true, false, 20)::cli_item_type,
		            	('foreignExchange', 'FX (Loss) & Gain', false, true, true, 30)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type
	            ] ::cli_section_type[],
            	null
	            ) ::form_definition_type,	            
	            ('cgiAnimationWithShoot', 'CGI\\Animation', 'CGI/Animation',
	            '[
				    {
				        "label": "Post production company",
				        "name": "postProductionCompany",
				        "type": "string",
				        "mandatory": false
				    },
				    {
				        "label": "Music company",
				        "name": "musicCompany",
				        "type": "string",
				        "mandatory": false
				    },
				    {
				        "label": "CGI/Animation company",
				        "name": "cgiAnimationCompany",
				        "type": "string",
				        "mandatory": true
				    },
				    {
				        "label": "Direct Billing",
				        "name": "directBilling",
				        "type": "checkbox",
				        "mandatory": true
				    }
				]'::json, ARRAY[
	            	('postProduction', 'Post Production Cost', 'Post Production Currency', null, 10,
		            	ARRAY[
		            	('offlineEdits', 'Offline edits', false, true, false, 0)::cli_item_type,
		            	('audioFinalization', 'Audio finalization', false, true, false, 10)::cli_item_type,
		            	('onlineVideoFinalization', 'Online video finalization', false, true, false, 20)::cli_item_type,
		            	('cgiAnimation', 'CGI/animation', false, true, false, 30)::cli_item_type,
		            	('pphMarkup', 'PPH mark up (if applicable)', false, true, false, 40)::cli_item_type		            	
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
	            	('agencyCosts', 'Agency Costs', null, null, 20,
		            	ARRAY[
		            	('agencyArtWorkPacks', 'Agency artwork/packs', false, true, false, 0)::cli_item_type,
		            	('agencyTravel', 'Agency travel', false, true, false, 10)::cli_item_type,
		            	('insurance', 'Insurance (if not covered by P&G)', false, true, false, 20)::cli_item_type,
		            	('musicExcludingUsageBuyout', 'Music', false, true, false, 30)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
	            	('Other', 'Other Costs', null, null, 30,
		            	ARRAY[
		            	('taxImportation', 'Tax (if applicable)', false, true, false, 0)::cli_item_type,
		            	('PandGInsurance', 'P&G insurance', false, true, false, 10)::cli_item_type,
		            	('technicalFee', 'Technical fee (if applicable)', true, true, false, 20)::cli_item_type,
		            	('foreignExchange', 'FX (Loss) & Gain', false, true, true, 30)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type
	            ] ::cli_section_type[],
            	null
	            ) ::form_definition_type
		] ::form_definition_type[]
		) ::production_detail_template_type,
		('audio', 
		ARRAY[
			('audioProduction', 'Audio Full Production', 'Full Production',
	        	'[
				    {
				        "label": "Recording Date",
				        "name": "recordingDate",
				        "type": "date",
				        "mandatory": true
				    },
				    {
				        "label": "Audio Company",
				        "name": "audioCompany",
				        "type": "dropdown",
				        "mandatory": true
				    },
				    {
				        "label": "Recording Country",
				        "name": "recordingCountry",
				        "type": "dropdown",
				        "mandatory": true
				    },
				    {
				        "label": "Recording City",
				        "name": "recordingCity",
				        "type": "dropdown",
				        "mandatory": true
				    },
				    {
				        "label": "Other Recording country",
				        "name": "otherRecordingCountry",
				        "type": "string",
				        "mandatory": false
				    },
				    {
				        "label": "Other Recording city",
				        "name": "otherRecordingCity",
				        "type": "string",
				        "mandatory": false
				    },
				    {
				        "label": "Recording Days",
				        "name": "recordingDays",
				        "type": "number",
				        "mandatory": true
				    },
				    {
				        "label": "Talent Company",
				        "name": "talentCompany",
				        "type": "dropdown",
				        "mandatory": [
				            "FinalActual"
				        ]
				    }
				]'::json,
	            ARRAY[
		            ('production', 'Studio Costs', 'Production Currency', null, 0,
		            	ARRAY[
		            	('talentFees', 'Talent fees', false, true, false, 0)::cli_item_type,
		            	('VoRecordingSessions', 'VO recording sessions', false, true, false, 10)::cli_item_type,
		            	('SoundDesignFinalMix', 'Sound design & final mix', false, true, false, 20)::cli_item_type,
		            	('MusicRecordingSessions', 'Music composition & recording sessions', false, true, false, 30)::cli_item_type,
		            	('StudioMarkUp', 'Studio mark up (if applicable)', false, true, false, 40)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
	            	('otherCosts', 'Other Costs', null, null, 30,
		            	ARRAY[
		            	('taxImportation', 'Tax (if applicable)', false, true, false, 0)::cli_item_type,
		            	('PAndGInsurance', 'P&G insurance', false, true, false, 10)::cli_item_type,
		            	('technicalFee', 'Technical fee (if applicable)', true, true, false, 20)::cli_item_type,
		            	('foreignExchange', 'FX (Loss) & Gain', false, true, true, 30)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type
	            ] ::cli_section_type[],
            	null
	            ) ::form_definition_type,
	            ('audioPostProduction', 'Audio Post Production Only', 'Post Production Only',
	            '[
				    {
				        "label": "Recording Date",
				        "name": "recordingDate",
				        "type": "date",
				        "mandatory": false
				    },
				    {
				        "label": "Audio/Music Company",
				        "name": "audioCompany",
				        "type": "dropdown",
				        "mandatory": true
				    },
				    {
				        "label": "Recording Country",
				        "name": "recordingCountry",
				        "type": "dropdown",
				        "mandatory": false
				    },
				    {
				        "label": "Recording City",
				        "name": "recordingCity",
				        "type": "dropdown",
				        "mandatory": false
				    },
				    {
				        "label": "Other Recording country",
				        "name": "otherRecordingCountry",
				        "type": "string",
				        "mandatory": false
				    },
				    {
				        "label": "Other Recording city",
				        "name": "otherRecordingCity",
				        "type": "string",
				        "mandatory": false
				    },				    
				    {
				        "label": "Recording Days",
				        "name": "recordingDays",
				        "type": "number",
				        "mandatory": false
				    },
				    {
				        "label": "Talent Company",
				        "name": "talentCompany",
				        "type": "dropdown",
				        "mandatory": false
				    }
				]'::json, ARRAY[
	            	('postProduction', 'Studio Costs', 'Post Production Currency', null, 10,
		            	ARRAY[
		            	('talentFees', 'Talent fees', false, true, false, 0)::cli_item_type,
		            	('VoRecordingSessions', 'VO recording sessions', false, true, false, 10)::cli_item_type,
		            	('SoundDesignFinalMix', 'Sound design & final mix', false, true, false, 20)::cli_item_type,
		            	('MusicRecordingSessions', 'Music composition & recording sessions', false, true, false, 30)::cli_item_type,
		            	('StudioMarkUp', 'Studio mark up (if applicable)', false, true, false, 40)::cli_item_type            	
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
	            	('agencyCosts', 'Other Costs', null, null, 20,
		            	ARRAY[
		            	('taxIfApplicable', 'Tax (if applicable)', false, true, false, 0)::cli_item_type,
		            	('PAndGInsurance', 'P&G insurance', false, true, false, 10)::cli_item_type,
		            	('technicalFee', 'Technical fee (if applicable)', true, true, false, 20)::cli_item_type,
		            	('foreignExchange', 'FX (Loss) & Gain', false, true, true, 30)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type
	            ] ::cli_section_type[],
            	null
	            ) ::form_definition_type
		] ::form_definition_type[]
		) ::production_detail_template_type,
		('photography', 
		ARRAY[
			('stillImageProduction', 'Still Image Full Production', 'Full Production',
	        	'[
				    {
				        "label": "First Shoot Date",
				        "name": "firstShootDate",
				        "type": "date",
				        "mandatory": true
				    },
				    {
				        "label": "Shoot country",
				        "name": "primaryShootCountry",
				        "type": "string",
				        "mandatory": true
				    },
				    {
				        "label": "Shoot city",
				        "name": "primaryShootCity",
				        "type": "string",
				        "mandatory": true
				    },
				    {
				        "label": "Other Shoot country",
				        "name": "otherShootCountry",
				        "type": "string",
				        "mandatory": false
				    },
				    {
				        "label": "Other Shoot city",
				        "name": "otherShootCity",
				        "type": "string",
				        "mandatory": false
				    },
				    {
				        "label": "# of Shoot Days",
				        "name": "noShootDays",
				        "type": "number",
				        "mandatory": true
				    },
				    {
				        "label": "Photographer Company",
				        "name": "photographerCompany",
				        "type": "dropdown",
				        "mandatory": true
				    },
				    {
				        "label": "Photographer Name",
				        "name": "photographerName",
				        "type": "dropdown",
				        "mandatory": true
				    },
				    {
				        "label": "Retouching Company",
				        "name": "retouchingCompany",
				        "type": "dropdown",
				        "mandatory": true
				    },
				    {
				        "label": "Talent Company",
				        "name": "talentCompany",
				        "type": "dropdown",
				        "mandatory": [
				            "FinalActual"
				        ]
				    },
				    {
				        "label": "Standard Billing / Direct Billing",
				        "name": "directBilling",
				        "type": "checkbox",
				        "mandatory": true
				    }
				]'::json,
	            ARRAY[
		            ('production', 'Still Image Production Cost', 'Production Currency', null, 0,
		            	ARRAY[
		            	('preProduction', 'Preproduction', false, true, false, 0)::cli_item_type,
		            	('talentFees', 'Talent fees', false, true, false, 10)::cli_item_type,
		            	('photographer', 'Photographer', false, true, false, 20)::cli_item_type,
		            	('crewSalaries', 'Crew salaries', false, true, false, 30)::cli_item_type,
		            	('equiment', 'Equipment', false, true, false, 40)::cli_item_type,
		            	('locationStudioArtDepartment', 'Location/studio/art department/sets', false, true, false, 50)::cli_item_type,
		            	('travelExclAgency', 'Travel (excl. Agency)', false, true, false, 60)::cli_item_type,
		            	('productionInsuranceNotCovered', 'Insurance (if not covered by P&G)', false, true, false, 70)::cli_item_type,
		            	('markUp', 'Mark up (if applicable)', false, true, false, 80)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
		            ('postProduction', 'Still Image Post Production Cost', 'Post Production Currency', null, 10,
		            	ARRAY[
		            	('retouching', 'Retouching', false, true, false, 0)::cli_item_type,
		            	('artworkPacks', 'Artwork/packs', false, true, false, 10)::cli_item_type,
		            	('markUpIfApplicable', 'Mark up (if applicable)', false, true, false, 20)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
		            ('agencyCosts', 'Agency Costs', null, null, 20,
		            	ARRAY[
		            	('agencyArtworkPacks', 'Agency Artwork / Packs', false, true, false, 0)::cli_item_type,
		            	('agencyTravel', 'Agency Travel', false, true, false, 10)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
	            	('otherCosts', 'Other Costs', null, null, 30,
		            	ARRAY[
		            	('taxIfApplicable', 'Tax (if applicable)', false, true, false, 0)::cli_item_type,
		            	('PAndGInsurance', 'P&G insurance', false, true, false, 10)::cli_item_type,
		            	('technicalFee', 'Technical fee (if applicable)', true, true, false, 20)::cli_item_type,
		            	('foreignExchange', 'FX (Loss) & Gain', false, true, true, 30)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type
	            ] ::cli_section_type[],
            	null
	            ) ::form_definition_type,
	            ('stillImagePostProduction', 'Still Image Post Production Only', 'Post Production Only',
	            '[
				    {
				        "label": "Photographer Company",
				        "name": "photographerCompany",
				        "type": "dropdown",
				        "mandatory": false
				    },
				    {
				        "label": "Retouching Company",
				        "name": "retouchingCompany",
				        "type": "dropdown",
				        "mandatory": true
				    },
				    {
				        "label": "Talent Company",
				        "name": "talentCompany",
				        "type": "dropdown",
				        "mandatory": false
				    },
				    {
				        "label": "Standard Billing / Direct Billing",
				        "name": "directBilling",
				        "type": "checkbox",
				        "mandatory": true
				    }
				]'::json, ARRAY[
	            	('postProduction', 'Still Image Post Production Cost', 'Post Production Currency', null, 10,
		            	ARRAY[
		            	('retouching', 'Retouching', false, true, false, 0)::cli_item_type,
		            	('artworkPacks', 'Artwork/packs', false, true, false, 10)::cli_item_type,
		            	('markUpIfApplicable', 'Mark up (if applicable)', false, true, false, 20)::cli_item_type          	
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
	            	('agencyCosts', 'Agency Costs', null, null, 20,
		            	ARRAY[
		            	('agencyArtworkPacks', 'Agency Artwork / Packs', false, true, false, 0)::cli_item_type,
		            	('agencyTravel', 'Agency Travel', false, true, false, 10)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
		            ('otherCosts', 'Other Costs', null, null, 30,
		            	ARRAY[
		            	('taxIfApplicable', 'Tax (if applicable)', false, true, false, 0)::cli_item_type,
		            	('PAndGInsurance', 'P&G insurance', false, true, false, 10)::cli_item_type,
		            	('technicalFee', 'Technical fee (if applicable)', true, true, false, 20)::cli_item_type,
		            	('foreignExchange', 'FX (Loss) & Gain', false, true, true, 30)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type
	            ] ::cli_section_type[],
            	null
	            ) ::form_definition_type
		] ::form_definition_type[]
		) ::production_detail_template_type,
		('digital', 
		ARRAY[
			('digitalDevelopmentProduction', 'Digital Development Production', 'Full Production',
	        	'[
				    {
				        "label": "Digital Development Company",
				        "name": "digitalDevelopmentCompany",
				        "type": "dropdown",
				        "mandatory": true
				    },
				    {
				        "label": "Direct Billing",
				        "name": "directBilling",
				        "type": "checkbox",
				        "mandatory": true
				    }
				]'::json,
	            ARRAY[
		            ('production', 'Digital Production Cost', 'Production Currency', null, 0,
		            	ARRAY[
		            	('master', 'Master', false, true, false, 0)::cli_item_type,
		            	('adaptation', 'Adaptation', false, true, false, 10)::cli_item_type,
		            	('social', 'Social', false, true, false, 20)::cli_item_type,
		            	('projectManagementProducer', 'Project Management/Producer', false, true, false, 30)::cli_item_type,
		            	('digitalStrategyDesign', 'Digital Strategy/Design', false, true, false, 40)::cli_item_type,
		            	('development', 'Development', false, true, false, 50)::cli_item_type,
		            	('qualityAssurance', 'Quality Assurance', false, true, false, 60)::cli_item_type,
		            	('markUp', 'Mark up (if applicable)', false, true, false, 70)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
		            ('postProduction', 'Digital Post Production Cost', 'Post Production Currency', null, 10,
		            	ARRAY[
		            	('saasLicensing', 'SAAS Licensing', false, true, false, 0)::cli_item_type,
		            	('ugcIntegration', 'UGC Integration', false, true, false, 10)::cli_item_type,
		            	('virtualReality', 'Virtual Reality', false, true, false, 20)::cli_item_type,
		            	('augmentedReality', 'Augmented Reality', false, true, false, 30)::cli_item_type,
		            	('hostingMicrosite', 'Hosting Microsite', false, true, false, 40)::cli_item_type,
		            	('markUpIfapplicable', 'Mark up (if applicable)', false, true, false, 50)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
	            	('otherCosts', 'Other Costs', null, null, 30,
		            	ARRAY[
		            	('taxImportation', 'Tax (if applicable)', false, true, false, 0)::cli_item_type,
		            	('PAndGInsurance', 'P&G insurance', false, true, false, 10)::cli_item_type,
		            	('technicalFee', 'Technical fee (if applicable)', true, true, false, 20)::cli_item_type,
		            	('foreignExchange', 'FX (Loss) & Gain', false, true, true, 30)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type
	            ] ::cli_section_type[],
            	null
	            ) ::form_definition_type
		] ::form_definition_type[]
		) ::production_detail_template_type
	] ::production_detail_template_type[], null);
	

-- Buyout Cost
select fn_add_cost_template_version(1, 
('costDetails', 'Cost Details', 
'[
    {
        "label": "Agency Name",
        "name": "agencyName",
        "type": "string"
    },
    {
        "label": "Agency Location",
        "name": "agencyLocation",
        "type": "string"
    },
    {
        "label": "Agency producer/art buyer",
        "name": "agencyProducerArtBuyer",
        "type": "string"
    },
    {
        "label": "Budget Region",
        "name": "budgetRegion",
        "type": "string"
    },
    {
        "label": "Campaign",
        "name": "brand",
        "type": "string"
    },
    {
        "label": "Content Type",
        "name": "contentType",
        "type": "string"
    },
    {
        "label": "Production Type",
        "name": "productionType",
        "type": "string"
    },
    {
        "label": "Target Budget",
        "name": "targetBudget",
        "type": "number"
    },
    {
        "label": "Agency Tracking Number",
        "name": "contentType",
        "type": "string"
    },
    {
        "label": "Organisation",
        "name": "organisation",
        "type": "string"
    },
    {
        "label": "Agency Payment Currency",
        "name": "agencyCurrency",
        "type": "string"
    }
]'::json) ::cost_template_type, 
	ARRAY[('video', 
		ARRAY[
			('fullProductionWithShoot', 'Full Production', 'Full Production',
	        	'[]'::json,
	            ARRAY[
		            ('usageBuyout', 'Usage/Buyout/Contract Costs', 'Usage/Buyout/Contract Currency', null, 0,
		            	ARRAY[
		            	('usageBuyoutFee', 'Usage/Residuals/Buyout Fee', false, true, false, 0)::cli_item_type,
		            	('negotiationBrokerAgencyFee', 'Negotiation/broker agency fee', false, true, false, 10)::cli_item_type,
		            	('otherServicesAndFees', 'Other services & fees', false, true, false, 20)::cli_item_type,
		            	('bonusCelebrityOnly', 'Bonus (celebrity only)', false, true, false, 30)::cli_item_type,
		            	('baseCompensation', 'Base Compensation', false, true, false, 40)::cli_item_type,
		            	('pensionAndHealth', 'Pension & Health (e.g. SAG)', false, true, false, 50)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
	            	('otherCosts', 'Other Costs', null, null, 30,
		            	ARRAY[		            	
		            	('technicalFee', 'Technical fee (if applicable)', true, true, false, 0)::cli_item_type,
		            	('foreignExchange', 'FX (Loss) & Gain', false, true, true, 10)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type
	            ] ::cli_section_type[],
            	null
	            ) ::form_definition_type
		] ::form_definition_type[]
		) ::production_detail_template_type
	] ::production_detail_template_type[], 
	ARRAY[
		('buyoutDetails', 'Usage/buyout Details', null,
        	'[
			    {
			        "label": "Name",
			        "name": "name",
			        "type": "string"
			    },
			    {
			        "label": "Name of licensor",
			        "name": "nameOfLicensor",
			        "type": "string"
			    },
			    {
			        "label": "Airing countries",
			        "name": "airingCountries",
			        "type": "string[]"
			    },
			    {
			        "label": "Touchpoints",
			        "name": "touchpoints",
			        "type": "string[]"
			    }
			]'::json,
            null,
        	ARRAY[
	        	('contract', 'Contract',
	        	'[
				    {
				        "label": "Exclusivity",
				        "name": "exclusivity",
				        "type": "string"
				    },
				    {
				        "label": "Exclusivity category",
				        "name": "exclusivityCategory",
				        "type": "string"
				    },
				    {
				        "label": "Start date",
				        "name": "startDate",
				        "type": "date"
				    },
				    {
				        "label": "End date",
				        "name": "endDate",
				        "type": "date"
				    },
				    {
				        "label": "Contract ID number",
				        "name": "contractId",
				        "type": "string"
				    }
				]'::json
				)::form_section_definition_type
			] ::form_section_definition_type[]
            ) ::form_definition_type,
            ('negotiatedTerms', 'Negotiated Terms', null,
        	'[
			    {
			        "label": "Produced asset",
			        "name": "producedAsset",
			        "type": "string"
			    }
			]'::json,
            null,
        	ARRAY[
	        	('Talent Decision Rights', 'talentDecisionRights',
	        	'[
				    {
				        "label": "Makeup artist",
				        "name": "makeupArtist",
				        "type": "string"
				    },
				    {
				        "label": "Hair stylist",
				        "name": "hairStylist",
				        "type": "string"
				    },
				    {
				        "label": "Nail artist",
				        "name": "nailArtist",
				        "type": "string"
				    },
				    {
				        "label": "Wardrobe artist",
				        "name": "wardrobeArtist",
				        "type": "string"
				    }
				]'::json
				)::form_section_definition_type,
				('Entourage Travel', 'entourageTravel',
	        	'[
				    {
				        "label": "Celebrity",
				        "name": "celebrity",
				        "type": "string"
				    },
				    {
				        "label": "Manager",
				        "name": "manager",
				        "type": "string"
				    },
				    {
				        "label": "Glam squad",
				        "name": "glamSquad",
				        "type": "string"
				    },
				    {
				        "label": "Security",
				        "name": "security",
				        "type": "string"
				    }
				]'::json
				)::form_section_definition_type
			] ::form_section_definition_type[]
            ) ::form_definition_type
	] ::form_definition_type[]);

-- Distribution and Trafficking Cost
select fn_add_cost_template_version(2, 
('costDetails', 'Cost Details', 
'[
    {
        "label": "Agency Name",
        "name": "agencyName",
        "type": "string"
    },
    {
        "label": "Agency Location",
        "name": "agencyLocation",
        "type": "string"
    },
    {
        "label": "Agency producer/art buyer",
        "name": "agencyProducerArtBuyer",
        "type": "string"
    },
    {
        "label": "Budget Region",
        "name": "budgetRegion",
        "type": "string"
    },
    {
        "label": "Target Budget",
        "name": "targetBudget",
        "type": "number"
    },
    {
        "label": "Agency Tracking Number",
        "name": "contentType",
        "type": "string"
    },
    {
        "label": "Organisation",
        "name": "organisation",
        "type": "string"
    },
    {
        "label": "Agency Currency",
        "name": "agencyCurrency",
        "type": "string"
    }
]'::json) ::cost_template_type, 
	ARRAY[('Trafficking', 
		ARRAY[
			('Trafficking', 'Trafficking', null,
	        	'[]'::json,
	            ARRAY[
		            ('distributionCosts', 'Distribution Costs', 'Trafficking Distribution Currency', null, 0,
		            	ARRAY[
		            	('distributionCosts', 'Trafficking/Distribution Costs', false, true, false, 0)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type,
	            	('otherCosts', 'Other Costs', null, null, 30,
		            	ARRAY[
		            	('taxIfApplicable', 'Tax (if applicable)', false, true, false, 0)::cli_item_type,
		            	('technicalFee', 'Technical fee (if applicable)', true, true, false, 10)::cli_item_type,
		            	('foreignExchange', 'FX (Loss) & Gain', false, true, true, 20)::cli_item_type
		            	] ::cli_item_type[]
		            ) ::cli_section_type
	            ] ::cli_section_type[],
            	null
	            ) ::form_definition_type
		] ::form_definition_type[]
		) ::production_detail_template_type
	] ::production_detail_template_type[], 
	null);
	
DROP TYPE cost_template_type CASCADE;
DROP TYPE production_detail_template_type CASCADE;
DROP TYPE cli_item_type CASCADE;
DROP TYPE cli_section_type CASCADE;
DROP TYPE form_section_definition_type CASCADE;
DROP TYPE form_definition_type CASCADE;

