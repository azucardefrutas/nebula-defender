"""Original offline synthesis. Python standard library only; no samples or external media."""
from pathlib import Path
from array import array
import math
import random
import wave

ROOT = Path(__file__).resolve().parents[1] / 'Assets/Nebula/Audio'
RATE = 22050
TAU = 2 * math.pi

def write(path, data):
    path.parent.mkdir(parents=True, exist_ok=True)
    peak = max(0.001, max(abs(x) for x in data))
    pcm = array('h', (int(max(-1, min(1, x / peak * 0.66)) * 32767) for x in data))
    with wave.open(str(path), 'wb') as output:
        output.setnchannels(1); output.setsampwidth(2); output.setframerate(RATE)
        output.writeframes(pcm.tobytes())

def tone(name, duration, kind, seed):
    rng = random.Random(seed)
    data = []
    phase = 0
    for i in range(int(duration * RATE)):
        t = i / RATE; u = t / duration
        envelope = min(1, t / 0.007) * (1-u)**2
        if kind == 'laser':
            f = 1100 * (1-u) + 140; phase += TAU*f/RATE
            value = (math.sin(phase)+0.2*math.sin(phase*2.01))*envelope
        elif kind == 'noise':
            phase += TAU * (90*(1-u)+25)/RATE
            value = (rng.uniform(-1,1)*0.6+math.sin(phase)*0.6)*envelope*math.exp(-u*2)
        elif kind == 'rise':
            phase += TAU*(260+u*900)/RATE
            value = (math.sin(phase)+0.3*math.sin(phase*1.5))*envelope
        else:
            notes = [60,64,67,72] if kind == 'major' else [60,58,55,48]
            note = notes[min(3,int(u*4))]
            f = 440*2**((note-69)/12); phase += TAU*f/RATE
            value = (math.sin(phase)+0.25*math.sin(phase*2))*envelope
        data.append(value)
    write(ROOT/'SFX'/f'{name}.wav',data)

def music(sector):
    beat = 0.6
    duration = 32 * beat
    roots = [48,44,51,46] if sector != 3 else [45,41,44,40]
    data = []
    for i in range(int(duration * RATE)):
        t=i/RATE; bar=int(t/(beat*8))%4; root=roots[bar]
        f=440*2**((root-69)/12)
        pad=(math.sin(TAU*f*t)+0.4*math.sin(TAU*f*1.5*t)+0.3*math.sin(TAU*f*2*t))*0.09
        bt=t%beat
        bass=math.sin(TAU*f/2*t)*math.exp(-bt*7)*0.11
        subdivision=beat/(2 if sector>0 else 1)
        tick=t%subdivision
        step=int(t/subdivision)
        note=[0,7,12,15,12,7,3,7][step%8]
        arp=math.sin(TAU*f*2**(note/12)*2*t)*math.exp(-tick*14)*0.065
        kick=math.sin(TAU*(45*bt+3*(1-math.exp(-bt*30))))*math.exp(-bt*24)*0.10 if sector>1 else 0
        fade=min(1,t/0.15,(duration-t)/0.15)
        data.append((pad+bass+arp+kick)*fade)
    write(ROOT/'Music'/f'Sector{sector}.wav',data)

if __name__ == '__main__':
    names=[('Laser',.13,'laser'),('HostileLaser',.21,'laser'),('Impact',.13,'noise'),('Explosion',.65,'noise'),('Shield',.6,'rise'),('Select',.07,'rise'),('Confirm',.15,'major'),('Complete',1.0,'major'),('Victory',2.0,'major'),('Defeat',1.5,'minor')]
    for seed,(name,duration,kind) in enumerate(names): tone(name,duration,kind,seed+910)
    write(ROOT/'SFX'/'Engine.wav',[math.sin(TAU*55*i/RATE)*.6+math.sin(TAU*110*i/RATE)*.2 for i in range(RATE*2)])
    for sector in range(4): music(sector)
    print('Generated 4 original music loops and 11 original effects.')
