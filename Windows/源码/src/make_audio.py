"""Original, deterministic PCM music and sound design. Standard library only."""
import math
import wave
from array import array
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / 'assets'
RATE = 22050

def note(buffer, start, midi, length, gain=.13):
    freq = 440 * 2 ** ((midi - 69) / 12)
    offset = int(start * RATE)
    count = int(length * RATE)
    for i in range(count):
        t = i / RATE
        env = min(1, t / .014) * math.exp(-t * 2.7 / length) * min(1, (length-t)/.15)
        signal = math.sin(2*math.pi*freq*t) + .18*math.sin(4*math.pi*freq*t) + .055*math.sin(6*math.pi*freq*t)
        buffer[(offset+i) % len(buffer)] += signal * env * gain

def save(name, data):
    peak = max(abs(x) for x in data) or 1
    factor = min(1, .65 / peak)
    samples = array('h', (int(max(-1,min(1,x*factor))*32767) for x in data))
    with wave.open(str(ROOT / name), 'wb') as out:
        out.setnchannels(1); out.setsampwidth(2); out.setframerate(RATE)
        out.writeframes(samples.tobytes())

ROOT.mkdir(exist_ok=True)
beat = 60/72
duration = 64*beat
data = array('f', [0]) * int(duration * RATE)
chords = [(50,57,62,66),(47,54,59,62),(43,50,55,59),(45,52,57,61)]
melody = [74,78,81,78,76,74,69,71,74,76,78,81,78,76,74,69,
          71,74,78,74,69,71,74,76,78,81,83,81,78,76,74,71]
for bar in range(16):
    chord = chords[bar%4]
    note(data, bar*4*beat, chord[0], 3.8*beat, .065)
    for j in range(4):
        note(data,(bar*4+j)*beat,chord[1+j%3]+12,1.8*beat,.055)
    for j in range(2):
        start=(bar*4+j*2+.5)*beat
        pitch=melody[bar*2+j]
        note(data,start,pitch,2*beat,.085)
        note(data,start+.24,pitch,1.8*beat,.012)
save('meadow.wav',data)
for filename, notes in {'click.wav':[74], 'harvest.wav':[74,78,81], 'coin.wav':[81,86], 'water.wav':[81,78,74]}.items():
    data=array('f',[0])*int(RATE*.7)
    for i,pitch in enumerate(notes):
        note(data,.02+i*.08,pitch,.25,.12)
    save(filename,data)
print('Generated 53-second original meadow loop and 4 sound effects.')
