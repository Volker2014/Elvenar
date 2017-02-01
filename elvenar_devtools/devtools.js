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
    element.innerHTML = element.innerHTML + content + "\n";
}

function isChecked(id)
{
    var check = _window.document.getElementById(id);
    if (check == null) return false;
    return check.checked;
}

function setContentWithCheck(content, id)
{
    if (!isChecked(id + '_check')) return;
    setContent(content, id + '_text');
}

function setCityMap(content, user) {
    var id = "citymap";
    if (!isChecked(id + '_check')) return;
    setContent(user, id + '_label');
    setContent(content, id + '_text');
}

function createGuild(responseData)
{
    line = "Gilde: " + responseData.name + "\n";
    for (var i = 0; i < responseData.members.length; i++)
    {
        var member = responseData.members[i];
        line = line + member.player.name + " " + member.score + "\n";
    }
    setContentWithCheck(line, "guild");
}

function createTournament(responseData)
{
    if (responseData.contributors == undefined) return;
    var line = "";
    for (var i = 0; i < responseData.contributors.length; i++)
    {
        var contributor = responseData.contributors[i];
        line = line + contributor.player.name + " " + contributor.score + "\n";
    }
    setContentWithCheck(line, "tournament");
}

function createRanking(responseData)
{
    if (responseData.rankings == undefined) return;
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
        line = line + name + " " + (ranking.points || 0) + "\n";
    }
    setContentWithCheck(line, "ranking");
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
    setCityMap(encoding, userdata[usernameProp]);
}

function createEnity(entity) {
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

function createResearch(research)
{
    if (research.childrenIds !== undefined)
    {
        for (var i = 0; i < research.childrenIds.length; i++)
            addContent(research.id + " -> " + research.childrenIds[i] + ";", "research_text");
    }
    addContent(research.id + ' [label="' + (research.name || research.id) + '"];', "research_text");
}

function showProps(object)
{
    if (!isChecked("development_check")) return;
    Object.keys(object).forEach(function (key) {
        addContent(key + " " + object[key], "development_text");
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
                createCityMap(serverResponse[i].responseData, "user_data", "user_name");
            else if (serverResponse[i].requestClass == 'OtherPlayerService')
                createCityMap(serverResponse[i].responseData, "other_player", "name");
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
                    addContent("id;name;race;type;level;width;length;construction_time;rankingPoints;upgrade;money;population;" +
                        "demand_for_happiness;mana;production;option;time;scrolls;mana;money;supplies;population", "cityentities_text");
                }
                createEnity(serverResponse[i]);
            }
        }
        else if (serverResponse[i].__class__ == 'ResearchTechnologyConfigVO') 
        {
            if (isChecked("research_check")) 
            {
                if (lastClass != 'ResearchTechnologyConfigVO')
                {
                    addContent("digraph elvenar_research {", "research_text");
                }
                createResearch(serverResponse[i]);
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
});
