var _window; // Going to hold the reference to panel.html's `window`

function addLine(content)
{
    _window.document.write(content + "<br/>");
}

function copyToClipboard(text) {
    if (window.clipboardData && window.clipboardData.setData) {
        // IE specific code path to prevent textarea being shown while dialog is visible.
        return clipboardData.setData("Text", text);

    } else if (document.queryCommandSupported && document.queryCommandSupported("copy")) {
        var textarea = document.createElement("textarea");
        textarea.textContent = text;
        textarea.style.position = "fixed";  // Prevent scrolling to bottom of page in MS Edge.
        document.body.appendChild(textarea);
        textarea.select();
        try {
            return document.execCommand("copy");  // Security exception may be thrown by some browsers.
        } catch (ex) {
            console.warn("Copy to clipboard failed.", ex);
            return false;
        } finally {
            document.body.removeChild(textarea);
        }
    }
}

function createGuild(responseData)
{
    addLine("Gilde: " + responseData.name);
    for (var i = 0; i < responseData.members.length; i++)
    {
        var member = responseData.members[i];
        addLine(member.player.name + ": " + member.score);
    }
    addLine("");
}

function createTournament(responseData)
{
    if (responseData.contributors == undefined) return;
    addLine("Turnier");
    for (var i = 0; i < responseData.contributors.length; i++)
    {
        var contributor = responseData.contributors[i];
        addLine(contributor.player.name + ": " + contributor.score);
    }
    addLine("");
}

function createRanking(responseData)
{
    if (responseData.rankings == undefined) return;
    addLine("Turnier");
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
        addLine(name + ": " + (ranking.points || 0));
    }
    addLine("");
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
    addLine("CityMap for Elvenar Architect of " + userdata.name);
    addLine(encoding);
    copyToClipboard(encoding);
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

    addLine(line);
}

function showProps(object)
{
    addLine("-----------------");
    Object.keys(object).forEach(function (key) {
        addLine(key + " " + object[key]);
    });
}

chrome.devtools.panels.create('Elvenar', '/icon.png', '/panel.html', function(extensionPanel) 
{	
    chrome.devtools.network.onRequestFinished.addListener(function(request) 
    {
        if (request.response.content.mimeType !== 'application/json') return;
        request.getContent(function (content, encoding)
        {
            serverResponse = JSON.parse(content);
            var lastClass = "";
            for (var i = 0; i < serverResponse.length; i++) 
            {
                if (serverResponse[i].__class__ == 'ServerResponseVO')
                {
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
                        addLine(serverResponse[i].requestClass);
                }
                else if (serverResponse[i].__class__ == 'CityEntityVO')
                {
                    if (lastClass == "")
                    {
                        addLine("CityEntities");
                        addLine("id;name;race;type;level;width;length;construction_time;rankingPoints;upgrade;money;population;" +
                            "demand_for_happiness;mana;production;option;time;scrolls;mana;money;supplies;population");
                    }
                    createEnity(serverResponse[i]);
                }
                else
                    addLine(serverResponse[i].__class__);
                lastClass = serverResponse[i].__class__;
            }
        });
    });
    
    extensionPanel.onShown.addListener(function tmp(panelWindow) {		
        extensionPanel.onShown.removeListener(tmp); // Run once only
        _window = panelWindow;
    });
    
});
