# -*- coding: utf-8 -*-
"""이 디자인을 쓰는 덱을 어두운 채로 한 단계 밝힌다.

QA 덱에 두 번에 나눠 적용한 것(brighten_text.py → lighten_theme.py)을
한 번에 하는 판이다. 클라이언트 덱이 QA 덱의 원본이라 팔레트가 같아
같은 맵이 그대로 듣는다.

    uv run --python 3.12 --with python-pptx python tools/deck/lighten_deck.py "<덱 경로>"
    ... --dry   바꾸지 않고 무엇이 바뀔지만 본다

두 가지가 핵심이다.

**배경을 밝히면 밝은 글자의 대비는 오히려 떨어진다.** 그래서 배경은 명도
+0.04, 글자는 +0.05 로 글자를 더 올린다. 여기에 원래 너무 어두웠던 보라-회색
셋(#5C5A72 · #6F6C88 · #918EA6)은 한 칸씩 더 끌어올린다.

**같은 색이 역할에 따라 다르게 쓰인다.** #14141F 은 슬라이드 배경 채움이면서
주황 카드 위 글자이고, #918EA6 은 채움 · 선 · 글자 셋 다로 쓰인다. 그래서
채움 · 선 · 글자 맵을 따로 둔다. 주황 위 어두운 글자는 그대로 두는데, 주황이
밝아지므로 대비가 7.92 에서 8.88 로 저절로 오른다.

색상(H)과 채도(S)는 건드리지 않는다. 주황·초록 강조와 게임 스크린샷이 지금
인상을 유지해야 하기 때문이다. 초록만 명도를 올리면 채도가 죽어 예외로 더 올린다.
"""
import shutil
import sys
from pathlib import Path

from pptx import Presentation
from pptx.dml.color import RGBColor
from pptx.enum.dml import MSO_FILL

FILL = {
    "14141F": "1C1C2B",   # 슬라이드 배경판
    "1A1A28": "222234",
    "1E1E2C": "262638",
    "20202F": "28283B",   # 가장 밝은 카드 · 표 칸
    "2A2136": "342943",
    "918EA6": "9C99AF",
    "FF8C4D": "FF9C67",
    "FF5A6E": "FF6E80",
}

LINE = {
    "3A3852": "454261",
    "5D5B6F": "69667D",
    "6F6C88": "7C7995",
    "918EA6": "9F9CB1",
    "FF8C4D": "FF9C67",
    "FF5A6E": "FF7484",
}

# 원래 어두웠던 셋은 두 칸, 나머지는 한 칸
TEXT = {
    "5C5A72": "9F9CB1",
    "6F6C88": "B4B2C2",
    "918EA6": "CAC9D4",
    "FF5A6E": "FFA9B6",
    "EDEAE4": "F7F6F3",
    "D9D9D9": "E6E6E6",
    "D8D8D8": "E5E5E5",
    "C7C7C9": "D4D4D6",
    "BEBEC8": "CCCCD4",
    "A8A5BC": "B6B4C7",
    "6ECB63": "87D47E",   # 초록만 +0.07
    "FF8C4D": "FF9C67",
}

KEEP = {"14141F"}         # 주황 카드 위 어두운 글자


def main(argv):
    args = [a for a in argv if not a.startswith("--")]
    dry = "--dry" in argv
    if not args:
        print("덱 경로를 달라."); return 1
    deck = Path(args[0])
    if not deck.exists():
        print("없는 파일:", deck); return 1

    prs = Presentation(deck)
    n = dict.fromkeys(("채움", "표 칸", "선", "글자", "그대로 둔 글자"), 0)

    def set_fill(obj, key):
        try:
            if obj.fill.type != MSO_FILL.SOLID:
                return
            cur = str(obj.fill.fore_color.rgb).upper()
        except Exception:
            return
        if cur in FILL:
            n[key] += 1
            if not dry:
                obj.fill.fore_color.rgb = RGBColor.from_string(FILL[cur])

    for slide in prs.slides:
        for shape in slide.shapes:
            set_fill(shape, "채움")
            try:
                if shape.line.fill.type == MSO_FILL.SOLID:
                    cur = str(shape.line.color.rgb).upper()
                    if cur in LINE:
                        n["선"] += 1
                        if not dry:
                            shape.line.color.rgb = RGBColor.from_string(LINE[cur])
            except Exception:
                pass

            frames = []
            if shape.has_table:
                for row in shape.table.rows:
                    for cell in row.cells:
                        set_fill(cell, "표 칸")
                        frames.append(cell.text_frame)
            if shape.has_text_frame:
                frames.append(shape.text_frame)

            for tf in frames:
                for para in tf.paragraphs:
                    for run in para.runs:
                        if not run.text.strip():
                            continue
                        color = run.font.color
                        try:
                            if color is None or color.type is None or color.rgb is None:
                                continue
                            cur = str(color.rgb).upper()
                        except Exception:
                            continue
                        if cur in KEEP:
                            n["그대로 둔 글자"] += 1
                        elif cur in TEXT:
                            n["글자"] += 1
                            if not dry:
                                color.rgb = RGBColor.from_string(TEXT[cur])

    for k, v in n.items():
        print("  %-12s %4d" % (k, v))
    if dry:
        print("\n--dry 라서 저장하지 않았다.")
    else:
        bak = deck.with_suffix(deck.suffix + ".bak")
        if not bak.exists():
            shutil.copy2(deck, bak)
            print("\n백업: %s" % bak.name)
        prs.save(deck)
        print("저장: %s" % deck.name)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
