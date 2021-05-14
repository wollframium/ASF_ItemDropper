##
## download submodule
##
if [[ ! -d ArchiSteamFarm/ArchiSteamFarm ]]; then
  git submodule update --init
fi

##
## update submodule to latest tag 
##
git submodule foreach "git fetch origin; git checkout $(git rev-list --tags --max-count=1);"

git submodule foreach "git describe --tags;"

##
## wipe out old build
##

if [[ -d ./out ]]; then
  rm -rf ./out
  rm -rf ./ASFItemDropper/bin
  rm -rf ./ASFItemDropper/obj
  rm -rf ./ASF_ItemDropper.zip
fi

##
## release generic version
##

dotnet publish -c "Release" -f "net5.0" -o "out/generic" "/p:LinkDuringPublish=false"

7z a -tzip -mx7 ./ASF_ItemDropper.zip ./out/generic/ASFItemDropper.*
