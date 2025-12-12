#!/bin/bash

BASE_TEAM_URL="https://media.formula1.com/content/dam/fom-website/teams/2024"
BASE_FLAG_URL="https://media.formula1.com/content/dam/fom-website/2018-redesign-assets/Flags%2016x9"

# Teams
declare -a teams=("red-bull-racing" "mercedes" "ferrari" "mclaren" "aston-martin" "alpine" "williams" "rb" "kick-sauber" "haas-f1-team")

for team in "${teams[@]}"; do
    echo "Downloading $team logo..."
    curl -s -o "wwwroot/img/teams/$team.png" "$BASE_TEAM_URL/$team-logo.png"
    # Check if valid image (size > 0 and not error page)
    if [[ $(find "wwwroot/img/teams/$team.png" -size +100c) ]]; then
        echo "OK"
    else
        echo "Failed $team"
        rm "wwwroot/img/teams/$team.png"
    fi
done

# Flags
declare -a flags=("bahrain" "saudi-arabia" "australia" "japan" "china" "united-states" "italy" "monaco" "canada" "spain" "austria" "great-britain" "hungary" "belgium" "netherlands" "azerbaijan" "singapore" "mexico" "brazil" "qatar" "abu-dhabi")

for flag in "${flags[@]}"; do
    echo "Downloading $flag flag..."
    # Note: The flag URL has .transform/2col/image.png which might be dynamic resizing. 
    # I'll try the base png first, if not, the transform one.
    # Actually the previous code used the transform url.
    # Let's try the direct png first.
    curl -s -o "wwwroot/img/flags/$flag.png" "$BASE_FLAG_URL/$flag-flag.png"
    
    if [[ ! $(find "wwwroot/img/flags/$flag.png" -size +100c) ]]; then
        echo "Retrying with transform URL..."
        curl -s -o "wwwroot/img/flags/$flag.png" "$BASE_FLAG_URL/$flag-flag.png.transform/2col/image.png"
    fi

    if [[ $(find "wwwroot/img/flags/$flag.png" -size +100c) ]]; then
        echo "OK"
    else
        echo "Failed $flag"
        rm "wwwroot/img/flags/$flag.png"
    fi
done
