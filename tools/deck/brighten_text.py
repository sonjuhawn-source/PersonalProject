# -*- coding: utf-8 -*-
"""QA 포트폴리오 덱의 어두운 글자색을 밝은 쪽으로 옮긴다.

배경이 #14141F 계열(카드는 #1A1A28 · #1E1E2C · #20202F)인 어두운 덱이라,
보라-회색 계열 중 어두운 셋이 배경에 묻혀 있었다. WCAG 명도 대비로 재서
미달인 것만 올린다.

바꾸는 색은 **색상(H)과 채도(S)를 그대로 두고 명도(L)만 올린 것**이다.
보색을 새로 넣지 않는다 — 문제는 색상이 아니라 명도이고, 새 색을 들이면
주황(#FF8C4D)과 초록(#6ECB63)으로 짜인 기존 강조 체계가 깨진다.

    uv run --python 3.12 --with python-pptx python tools/deck/brighten_text.py
    ... --dry   바꾸지 않고 무엇이 바뀔지만 본다

표와 레이아웃·마스터는 손대지 않는다. 확인한 결과 거기에는 미달 색이 없다.
"""
import sys
from pathlib import Path

from pptx import Presentation
from pptx.dml.color import RGBColor

DECK = Path.home() / "Desktop" / "손주환 2차 개인프로젝트 QA 포트폴리오.pptx"

# 원본 → 바꿀 색. 원본 기준으로 한 번에 옮긴다 (순차로 하면 918EA6 이 두 번 바뀐다)
REMAP = {
    "5C5A72": ("918EA6", "푸터 · 쪽번호",      "2.75 → 5.76"),
    "6F6C88": ("A6A4B7", "작은 보조 설명",     "3.19 → 6.58"),
    "918EA6": ("BCBBC9", "본문 보조 (최다)",   "5.06 → 8.47"),
    "FF5A6E": ("FF8FA0", "빨강 강조",          "5.68 → 7.93"),
}


def runs_of(shape):
    if shape.has_text_frame:
        for para in shape.text_frame.paragraphs:
            for run in para.runs:
                yield run


def main(argv):
    dry = "--dry" in argv
    prs = Presentation(DECK)
    hit = {k: 0 for k in REMAP}
    pages = {k: set() for k in REMAP}

    for i, slide in enumerate(prs.slides, 1):
        for shape in slide.shapes:
            for run in runs_of(shape):
                if not run.text.strip():
                    continue
                color = run.font.color
                try:
                    if color is None or color.type is None or color.rgb is None:
                        continue
                    cur = str(color.rgb).upper()
                except Exception:
                    continue
                if cur not in REMAP:
                    continue
                hit[cur] += 1
                pages[cur].add(i)
                if not dry:
                    color.rgb = RGBColor.from_string(REMAP[cur][0])

    for src, (dst, role, delta) in REMAP.items():
        ps = ",".join(str(p) for p in sorted(pages[src]))
        print("  #%s → #%s  %-16s %-14s %3d런  쪽 %s"
              % (src, dst, role, delta, hit[src], ps))
    if dry:
        print("\n--dry 라서 저장하지 않았다.")
    else:
        prs.save(DECK)
        print("\n저장했다: %s" % DECK.name)


if __name__ == "__main__":
    main(sys.argv[1:])
