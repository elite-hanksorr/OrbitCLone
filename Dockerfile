FROM mono:latest as builder
WORKDIR /src
COPY . /src
RUN msbuild /t:Restore
RUN msbuild /t:Build /p:Configuration=Release

FROM mono:latest
COPY --from=builder /src/OrbitCLone/bin/DesktopGL/AnyCPU/Release/. /pkg
ENTRYPOINT ["mono", "/pkg/OrbitCLone.exe"]
