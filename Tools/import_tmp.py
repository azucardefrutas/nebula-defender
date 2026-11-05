"""Unpack the installed Unity TMP essentials synchronously, preserving original GUIDs and licenses."""
from pathlib import Path, PurePosixPath
import argparse
import tarfile

ROOT=Path(__file__).resolve().parents[1]
DEFAULT=Path(r'C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Data\Resources\PackageManager\BuiltInPackages\com.unity.ugui\Package Resources\TMP Essential Resources.unitypackage')

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--package',type=Path,default=DEFAULT);args=parser.parse_args()
    with tarfile.open(args.package) as archive:
        members={member.name.lstrip('./'):member for member in archive if member.isfile()}
        count=0
        for name,member in members.items():
            if not name.endswith('/pathname'):continue
            relative=PurePosixPath(archive.extractfile(member).read().decode('utf-8'))
            if relative.parts[:2]!=('Assets','TextMesh Pro') or '..' in relative.parts:raise ValueError('Unexpected package path')
            destination=ROOT.joinpath(*relative.parts)
            prefix=name.rsplit('/',1)[0]
            asset=members.get(prefix+'/asset');meta=members.get(prefix+'/asset.meta')
            if asset:
                destination.parent.mkdir(parents=True,exist_ok=True)
                if not destination.exists():destination.write_bytes(archive.extractfile(asset).read());count+=1
            else:destination.mkdir(parents=True,exist_ok=True)
            metadata=Path(str(destination)+'.meta')
            if meta and not metadata.exists():metadata.write_bytes(archive.extractfile(meta).read())
        print(f'Imported {count} bundled TMP essential assets without overwriting existing files.')
