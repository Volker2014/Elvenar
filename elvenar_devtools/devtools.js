var _window; // Going to hold the reference to panel.html's `window`

function setContent(content, id)
{
    var element = _window.document.getElementById(id);
    if (element == null) return;
    element.innerHTML = content;
}

function addContent(content, id) {
    var element = _window.document.getElementById(id);
    if (element == null) return;
    if (element.innerHTML == null)
        element.innerHTML = "";
    element.innerHTML = element.innerHTML + content + "\n";
}

function isChecked(id)
{
    var check = _window.document.getElementById(id);
    if (check == null) return false;
    return check.checked;
}

function getOption(id)
{
    var select = _window.document.getElementById(id);
    if (select == null) return "";
    return select.value;
}

function setContentWithCheck(content, id)
{
    if (!isChecked(id + '_check')) return;
    setContent(content, id + '_text');
}

function setCityMap(content, user, usernameProp)
{
    var id = "citymap";
    if (!isChecked(id + '_check')) return;
    var label = user[usernameProp];
    if (user.location !== undefined)
        label = label + "(" + user.location.q + "," + user.location.r + ")";
    setContent(label, id + '_label');
    setContent(content, id + '_text');
}

function createGuild(responseData)
{
    line = "Gilde: " + responseData.name + "\n";
    for (var i = 0; i < responseData.members.length; i++)
    {
        var member = responseData.members[i];
        line = line + member.player.name + ";" + member.score + "\n";
    }
    setContentWithCheck(line, "guild");
}

function createTournament(responseData)
{
    if (responseData.contributors === undefined) return;
    var line = "";
    for (var i = 0; i < responseData.contributors.length; i++)
    {
        var contributor = responseData.contributors[i];
        line = line + contributor.player.name + ";" + contributor.score + "\n";
    }
    setContentWithCheck(line, "tournament");
}

function createRanking(responseData)
{
    if (getOption("service_select") != "ranking") return;
    if (responseData.rankings === undefined) return;
    var line = "";
    for (var i = 0; i < responseData.rankings.length; i++)
    {
        var ranking = responseData.rankings[i];
        var name;
        if (ranking.__class__ == "PlayerRankingVO")
            name = ranking.player.name;
        else if (ranking.__class__ == "GuildRankingVO")
            name = ranking.guild_info.name;
        else if (ranking.__class__ == "TournamentRankingVO")
            name = ranking.player.name;
        line = line + name + ";" + (ranking.points || 0) + "\n";
    }
    addContent(line, "service_text");
}

function createQuest(responseData)
{
    if (getOption("service_select") != "quest") return;
    for (var j = 0; j < responseData.length; j++)
    {
        var quest = responseData[j];
        var line = quest.title + ";";
        for (var i = 0; i < quest.successConditions.length; i++)
            line = line + ";" + quest.successConditions[i].description + ";" +
                (quest.successConditions[i].currentProgress || 0) + ";" + quest.successConditions[i].maxProgress;
        line = line + ";";
        for (var i = 0; i < quest.rewards.length; i++)
            line = line + ";" + quest.rewards[i].subType + ";" + quest.rewards[i].amount;
        addContent(line, "service_text");
    }
}

function createCityMap(responseData, userdataProp, usernameProp)
{
    var userdata = responseData[userdataProp];
    var cityMap = { entities: [], unlocked_areas: [] };
    for (var i = 0; i < responseData.city_map.entities.length; i++)
    {
        var entity = responseData.city_map.entities[i];
        var newEntity = { id: entity.id, cityentity_id: entity.cityentity_id, x: entity.x || 0, y: entity.y || 0 };
        cityMap.entities.push(newEntity);
    }
    for (var i = 0; i < responseData.city_map.unlocked_areas.length; i++)
    {
        var unlockarea = responseData.city_map.unlocked_areas[i];
        var newArea = { x: unlockarea.x || 0, y: unlockarea.y || 0, width: unlockarea.width, length: unlockarea.length };
        cityMap.unlocked_areas.push(newArea);
    }
    var table = { city_map: cityMap, user_data: { race: userdata.race}};
    var jsonCity = JSON.stringify(table);
    var encoding = btoa(jsonCity);
    setCityMap(encoding, userdata, usernameProp);
}

function createResources(responseData, userdataProp, usernameProp)
{
    if (!isChecked("resources_check")) return;

    var userdata = responseData[userdataProp];
    setContent(userdata[usernameProp], 'resources_label');

    var table = {
        timestamp: new Date().toLocaleString(),
        username: userdata[usernameProp],
        population: 
            {
                currentPopulation: responseData.city_population.currentPopulation,
                currentPopulationDemand: responseData.city_population.currentPopulationDemand,
            },
        culture: 
            {
                currentCulture: responseData.city_culture.currentCulture,
                currentCultureDemand: responseData.city_culture.currentCultureDemand,
                neededForCurrentLevel: responseData.city_culture.neededForCurrentLevel,
                neededForNextLevel: responseData.city_culture.neededForNextLevel,
                neighbourlyHelpCultureBonus: responseData.city_culture.neighbourlyHelpCultureBonus,
                currentCultureLevel: responseData.city_culture.cultureBonusLevels[responseData.city_culture.currentCultureLevel],
            },
        storage: {
            money: responseData.resources.resources.money,
            supplies: responseData.resources.resources.supplies,
            broken_shards: responseData.resources.resources.broken_shards,
            premium: responseData.resources.resources.premium,
            capacity_money: responseData.resources_cap.resources.money,
            capacity_supplies: responseData.resources_cap.resources.supplies,
            capacity_broken_shards: responseData.resources_cap.resources.broken_shards
        },
        goods:{
            marble: responseData.resources.resources.marble,
            steel: responseData.resources.resources.steel,
            planks: responseData.resources.resources.planks,
            crystal: responseData.resources.resources.crystal,
            scrolls: responseData.resources.resources.scrolls,
            silk: responseData.resources.resources.silk,
            elixir: responseData.resources.resources.elixir,
            magic_dust: responseData.resources.resources.magic_dust,
            gems: responseData.resources.resources.gems
        }

    };
    var jsonResources = JSON.stringify(table);
    var encoding = btoa(jsonResources);
    setContent(jsonResources, "resources_text");
}
//,"resources":{"relic_marble":247,"tg_hm":485,"hb_hr":5878,"b_gr4_aw1_shards":1,"hb_lr":35550,,"hb_hm":5774,"mana":33733,,"relic_scrolls":184,"orcs":22235,,"relic_steel":758,"relic_planks":79,"spell_good_production_boost_1":216,"population":6619,"orcs_shroomofwisdom":200,"spell_neighborly_help_boost_1":92,"hb_ma":12905,"tg_lm":15100,"hb_lm":39659,"fairies_soma":88,"relic_crystal":381,"fairies_cocoons":3657,"fairies_nightshade":1096,"fairies_dreamsheep":1400,"b_dwarfs_aw2_shards":28,"fairies_ambrosia":7,"relic_elixir":672,"b_dwarfs_aw1_shards":21,"b_humans_aw2_shards":51,"b_humans_aw1_shards":83,"spell_supply_production_boost_1":124,"fairies_sunflower":1136,"spell_knowledge_boost_1":68,,"relic_gems":162,"relic_magic_dust":355,"b_all_aw2_shards":4,","relic_silk":228,"orcs_psychoshroom":7810,"orcs_dung":216,"orcs_loot":128,"orcs_hardshroom":2800,"orcs_powershroom":3424,"__class__":"Dictionary"}}				

function createEnity(entity, race)
{
    if (entity.race != race) return;
    line = entity.id + ";" + entity.name + ";" + entity.race + ";" + entity.type + ";" + entity.level + ";" +
        entity.width + ";" + entity.length + ";" + (entity.construction_time || 0) + ";" + (entity.rankingPoints || 0);
    if (entity.upgradeRequirements !== undefined) {
        var resources = entity.upgradeRequirements.resources.resources;
        line = line + ";;" + (resources.money || 0) + ";" + (resources.population || 0) + ";" +
            (resources.demand_for_happiness || 0) + ";" + (resources.mana || 0);
    }
    else
        line = line + ";;;;;";
    if (entity.production !== undefined)
    {
        line = line + ";";
        for (var i = 0; i < entity.production.products.length; i++)
        {
            var product = entity.production.products[i];
            line = line + ";option" + product.production_option + ";" + (product.production_time || 0) + ";";
            var resources = product.requirements.resources.resources;
            line = line + (resources.scrolls || 0) + ";" + (resources.mana || 0) + ";" +
                (resources.money || 0) + ";" + (resources.supplies || 0) + ";" + (resources.population || 0);
        }
    }

    addContent(line, "cityentities_text");
}

function createResearch(research, race)
{
    if (research.race != race) return;
    if (research.childrenIds !== undefined)
    {
        for (var i = 0; i < research.childrenIds.length; i++)
            addContent(research.id + " -> " + research.childrenIds[i] + ";", "research_text");
    }
    addContent(research.id + ' [label="' + (research.name || research.id) + '"];', "research_text");
}

function createNeighborHelp(neighborHelp)
{
    var line = new Date(neighborHelp.timestamp * 1000).toLocaleString() + ";" + neighborHelp.type + ";";
    if (neighborHelp.other_player.guild_info !== undefined)
        line = line + neighborHelp.other_player.guild_info.name;
    line = line + ";" + neighborHelp.other_player.name + ";";
    line = line + neighborHelp.entityType;
    addContent(line, "service_text");
}

function createTradeAccepted(trade)
{
    var line = new Date(trade.timestamp * 1000).toLocaleString() + ";" + trade.type + ";";
    if (trade.other_player.guild_info)
        line = line + trade.other_player.guild_info.name;
    line = line + ";" + trade.other_player.name + ";";
    line = line + trade.need.good_id + ";" + trade.need.value; ";" + trade.offer.good_id + ";" + trade.offer.value;
    addContent(line, "service_text");
}

function createNotification(responseData)
{
    if (getOption("service_select") != "notification") return;
    setContent("", "service_text");
    for (var i = 0; i < responseData.length; i++)
    {
        if (responseData[i].__class__ == "NeighborlyHelpNotificationVO")
            createNeighborHelp(responseData[i]);
        else if (responseData[i].__class__ == "TradeAcceptedNotificationVO")
            createTradeAccepted(responseData[i]);
    }
}

function showProps(object)
{
    if (getOption("service_select") != "development") return;
    Object.keys(object).forEach(function (key) {
        addContent(key + " " + object[key], "service_text");
    });
}

function parseContent(content, encoding)
{
    serverResponse = JSON.parse(content);
    var lastClass = "";
    for (var i = 0; i < serverResponse.length; i++) {
        if (serverResponse[i].__class__ == 'ServerResponseVO') {
            if (serverResponse[i].requestClass == 'GuildService')
                createGuild(serverResponse[i].responseData);
            else if (serverResponse[i].requestClass == 'TournamentService')
                createTournament(serverResponse[i].responseData);
            else if (serverResponse[i].requestClass == 'RankingService')
                createRanking(serverResponse[i].responseData);
            else if (serverResponse[i].requestClass == 'StartupService')
            {
                createCityMap(serverResponse[i].responseData, "user_data", "user_name");
                createResources(serverResponse[i].responseData, "user_data", "user_name");
            }
            else if (serverResponse[i].requestClass == 'OtherPlayerService')
            {
                createCityMap(serverResponse[i].responseData, "other_player", "name");
                createResources(serverResponse[i].responseData, "other_player", "name");
            }
            else if (serverResponse[i].requestClass == 'QuestService')
                createQuest(serverResponse[i].responseData);
            else if (serverResponse[i].requestClass == 'NotificationService')
            {
                if (serverResponse[i].requestMethod == "getAllNotifications")
                    createNotification(serverResponse[i].responseData);
            }
            else if (serverResponse[i].requestClass !== undefined)
            {
                if (isChecked("development_check"))
                    addContent(serverResponse[i].requestClass, "development_text");
            }
        }
        else if (serverResponse[i].__class__ == 'CityEntityVO') 
        {
            if (isChecked("cityentities_check"))
            {
                if (lastClass != 'CityEntityVO')
                {
                    setContent("", "cityentities_text");
                    addContent("id;name;race;type;level;width;length;construction_time;rankingPoints;upgrade;money;population;" +
                        "demand_for_happiness;mana;production;option;time;scrolls;mana;money;supplies;population", "cityentities_text");
                }
                createEnity(serverResponse[i], getOption("cityentities_race"));
            }
        }
        else if (serverResponse[i].__class__ == 'ResearchTechnologyConfigVO') 
        {
            if (isChecked("research_check")) 
            {
                if (lastClass != 'ResearchTechnologyConfigVO')
                {
                    setContent("", "research_text");
                    addContent("digraph elvenar_research {", "research_text");
                }
                createResearch(serverResponse[i], getOption("research_race"));
            }
        }
        else if (lastClass == 'ResearchTechnologyConfigVO')
        {
            addContent("}", "research_text");
        }
        else if (isChecked("development_check"))
            addContent(serverResponse[i].__class__, "development_text");
        lastClass = serverResponse[i].__class__;
    }
    if (lastClass == 'ResearchTechnologyConfigVO')
    {
        addContent("}", "research_text");
    }
}

chrome.devtools.panels.create('Elvenar', 'icon.png', 'panel.html', function(extensionPanel) 
{	
    chrome.devtools.network.onRequestFinished.addListener(function(request) 
    {
        if (request.response.content.mimeType !== 'application/json') return;
        request.getContent(parseContent);
    });
    
    extensionPanel.onShown.addListener(function tmp(panelWindow) {		
        extensionPanel.onShown.removeListener(tmp); // Run once only
        _window = panelWindow;
    });

    //Fetching all available resources and filtering using name of script snippet added 
    chrome.devtools.inspectedWindow.getResources(function (resources)
    {

        // This function returns array of resources available in the current window

        for (i = 0; i < resources.length; i++)
        {

            // Matching with current snippet URL

            if (resources[i].url == "Script snippet #1")
            {
                resources[i].getContent(function (content, encoding)
                {

                    addContent("encoding is " + encoding, "development_text");
                    addContent("content is  " + content, "development_text");
                });
            }
        }

    });

    //This can be used for identifying when ever a new resource is added

    chrome.devtools.inspectedWindow.onResourceAdded.addListener(function (resource)
    {
        addContent("resources added " + resource.url, "development_text");
        addContent("resources content added " + resource.content, "development_text");
    });

    //This can be used to detect when ever a resource code is changed/updated

    chrome.devtools.inspectedWindow.onResourceContentCommitted.addListener(function (resource, content)
    {
        addContent("Resource Changed", "development_text");
        addContent("New Content  " + content, "development_text");
        addContent("New Resource  Object is " + resource, "development_text");
    });
});
