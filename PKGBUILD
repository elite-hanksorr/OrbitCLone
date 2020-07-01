pkgname=orbitclone
pkgver=0.0.0
pkgrel=1
pkgdesc='Cheap OrbitXL ripoff for the sake of dank AI learning'
url='git+https://github.com/elite-hanksorr/OrbitCLone'
arch=('x86_64')
license=()
depends=('mono')
makedepends=('mono-msbuild')
optdepends=('mono-tools')
provides=('orbtclone')
source=("$pkgname::$url")
sha256sums=('SKIP')

pkgver() {
  cd "$pkgname"
  ver="$(git describe --tags | sed 's/^v//;s/-/./g')"
  if [ -n "$ver" ]; then echo "$ver"; else echo 0.0.0; fi
}

prepare() {
  cd "$pkgname"
  msbuild /t:Restore
}

build() {
  cd "$pkgname"
  msbuild /t:Build /p:Configuration=Release
}

package() {
  install -dm755 "$pkgdir/opt/$pkgname"
  cp -a "$srcdir/$pkgname"/OrbitCLone/bin/DesktopGL/AnyCPU/Release/. "$pkgdir/opt/$pkgname"
  cp -a "$srcdir/$pkgname"/OrbitLearner/bin/Release/. "$pkgdir/opt/$pkgname"
}

# vim: ts=2 sw=2 et:
