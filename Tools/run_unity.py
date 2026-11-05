"""Run the installed Editor with real exit codes and retained logs. No downloads or installs."""
from pathlib import Path
import argparse
import os
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
DEFAULT_EDITOR = r'C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe'

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('mode',choices=['generate','edit-tests','play-tests','build','web-build','validate','idempotence'])
    parser.add_argument('--editor',default=os.environ.get('UNITY_EDITOR',DEFAULT_EDITOR))
    parser.add_argument('--graphics',action='store_true')
    parser.add_argument('--filter',help='Run only matching Unity test names (test modes only)')
    args=parser.parse_args()
    evidence=ROOT/'Documentation/Evidence';evidence.mkdir(parents=True,exist_ok=True)
    cmd=[args.editor,'-batchmode','-projectPath',str(ROOT),'-buildTarget','WebGL' if args.mode=='web-build' else 'StandaloneWindows64','-logFile',str(evidence/f'{args.mode}.log'),'--burst-disable-compilation']
    if not args.graphics:cmd.append('-nographics')
    if args.mode in ['edit-tests','play-tests']:
        cmd+=['-runTests','-testPlatform','EditMode' if args.mode=='edit-tests' else 'PlayMode','-assemblyNames','Nebula.EditModeTests' if args.mode=='edit-tests' else 'Nebula.PlayModeTests','-testResults',str(evidence/f'{args.mode}.xml')]
        if args.filter:cmd+=['-testFilter',args.filter]
    else:
        method={'generate':'DemoGenerator.GenerateDemo','build':'WindowsBuilder.BuildWindows','web-build':'WebBuilder.BuildWeb','validate':'ProjectValidator.Validate','idempotence':'ProjectValidator.VerifyIdempotence'}[args.mode]
        cmd+=['-quit','-executeMethod','NebulaDefender.Editor.'+method]
    print(f'Running Unity 6000.3 / {args.mode}. Log: {evidence/(args.mode+".log")}',flush=True)
    result=subprocess.run(cmd,cwd=ROOT)
    print(f'Unity process exit code: {result.returncode}',flush=True)
    return result.returncode

if __name__=='__main__':sys.exit(main())
