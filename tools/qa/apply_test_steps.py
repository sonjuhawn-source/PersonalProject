# -*- coding: utf-8 -*-
"""docs/qa/test-steps.md 의 테스트 스텝을 WeaponHero_TC.xlsx 로 옮긴다.

원본은 md 이고 엑셀은 파생물이다. 엑셀을 손으로 고치면 둘이 갈라지므로,
스텝이 바뀌면 md 를 고치고 이 스크립트를 다시 돌린다.

    uv run --python 3.12 --with openpyxl python tools/qa/apply_test_steps.py
    ... --pre        사전 조건 열도 같이 덮어쓴다 (기본은 스텝만)
    ... --no-note    요약 시트의 초안 표시를 넣지 않는다 (스텝 확정 후)

엑셀이 Excel 에서 열려 있으면 쓰기가 막힌다. 닫고 돌린다.
"""
import math
import re
import sys
import unicodedata
from pathlib import Path

import openpyxl

ROOT = Path(__file__).resolve().parents[2]
DRAFT = ROOT / "docs" / "qa" / "test-steps.md"
BOOK = ROOT / "docs" / "qa" / "WeaponHero_TC.xlsx"

CASES = ROOT / "docs" / "qa" / "test-cases.md"

COL_DETAIL = 4   # D 소분류(확인 항목) — 앞에 TC ID 가 붙어 있다
COL_PRE = 5      # E 사전 조건
COL_STEPS = 6    # F 테스트 스텝
COL_EXPECT = 7   # G 기대 결과 — 원본은 test-cases.md 다

ID = re.compile(r"TC-[A-Z]+-\d+")


def parse_draft(text):
    """{TC ID: (사전조건, 스텝)} 을 만든다.

    절 머리의 '공통 사전 조건' 은 그 절의 모든 항목에 상속된다.
    항목에 자기 사전 조건이 있으면 그것이 이긴다.
    """
    out = {}
    common = ""
    tc = None
    pre = ""
    steps = []
    in_block = False

    def flush():
        if tc:
            out[tc] = (pre or common, list(steps))

    for line in text.split("\n"):
        s = line.strip()

        if s.startswith("## "):            # 절이 바뀌면 공통 조건도 초기화
            flush()
            tc, pre, steps, common, in_block = None, "", [], "", False
            continue

        if s.startswith("공통 사전 조건"):
            common = for_sheet(s.split("—", 1)[1]) if "—" in s else ""
            continue

        if s.startswith("#### "):
            flush()
            m = ID.search(s)
            tc, pre, steps, in_block = (m.group(0) if m else None), "", [], False
            continue

        if tc and s.startswith("사전 조건") and "—" in s:
            pre = for_sheet(s.split("—", 1)[1])
            continue

        if s.startswith("```"):
            in_block = not in_block
            continue

        if in_block and tc and s:
            steps.append(clean(s))

    flush()
    return out


def wrapped_lines(text, width):
    """엑셀 열 너비에 맞춰 줄바꿈된 줄 수를 센다.

    너비 단위는 기본 글꼴의 '0' 폭이다. 한글·전각은 그 두 배로 본다.
    """
    if not text:
        return 1
    n = 0
    for line in str(text).split(chr(10)):
        w = sum(2 if unicodedata.east_asian_width(ch) in "WF" else 1 for ch in line)
        n += max(1, math.ceil(w / max(width - 1, 1)))
    return n


def clean(s):
    """마크다운 강조를 벗긴다."""
    return s.replace("**", "").replace("`", "").strip()


def parse_expects(text):
    """test-cases.md 의 표에서 {TC ID: 기대 결과} 를 뽑는다.

    기대 결과는 스텝 문서가 아니라 TC 문서가 원본이다. 표가 5열인 절과
    6열(선행 조건이 있는 절)이 섞여 있어 열 수로 자리를 고른다.
    """
    out = {}
    for line in text.split(chr(10)):
        if not line.startswith("| TC-"):
            continue
        c = [x.strip() for x in line.split("|")]
        m = ID.search(c[1])
        if not m:
            continue
        out[m.group(0)] = clean(c[4] if len(c) == 9 else c[3])
    return out


def for_sheet(s):
    """작업용 표시를 제출용 문장으로 바꾼다.

    [에디터] · [확인] · [자동] 은 이 문서를 읽는 나를 위한 표시다. 제출용
    엑셀에 그대로 들어가면 대괄호가 오타처럼 읽힌다. 에디터 표시만 뜻이
    남아야 하므로 문장으로 풀고 나머지는 지운다.
    """
    s = clean(s)                       # 마커가 백틱에 싸인 곳이 있어 먼저 벗긴다
    s = s.replace("[에디터] ", "에디터 수행 · ").replace("[에디터]", "에디터 수행")
    s = s.replace("[확인] ", "").replace("[자동] ", "")
    return s.strip(" ·")


def main(argv):
    write_pre = "--pre" in argv
    note = "--no-note" not in argv

    steps_by_tc = parse_draft(DRAFT.read_text(encoding="utf-8"))
    expects = parse_expects(CASES.read_text(encoding="utf-8"))
    wb = openpyxl.load_workbook(BOOK)
    ws = wb["TC 목록"]

    wrote = skipped = missing = 0
    unseen = set(steps_by_tc)

    for row in range(2, ws.max_row + 1):
        detail = ws.cell(row, COL_DETAIL).value
        if not detail:
            continue
        m = ID.search(str(detail))
        if not m:
            continue
        tc = m.group(0)
        unseen.discard(tc)

        if tc not in steps_by_tc:
            missing += 1
            print("  초안에 없음:", tc)
            continue

        pre, steps = steps_by_tc[tc]

        # 기대 결과는 항상 맞춘다. 2회전에서 TC 문구를 넓혔는데 엑셀이 따라오지
        # 않아 넷이 갈라져 있었다 — 파생물은 손으로 고치지 않고 다시 뽑는다.
        want = expects.get(tc)
        if want and (ws.cell(row, COL_EXPECT).value or "").strip() != want:
            print("  기대 결과 갱신:", tc)
            ws.cell(row, COL_EXPECT).value = want

        if not steps:                       # N/A 항목은 비운다
            ws.cell(row, COL_STEPS).value = None
            ws.row_dimensions[row].height = None
            skipped += 1
            continue

        cell = ws.cell(row, COL_STEPS)
        cell.value = "\n".join(steps)
        # 행을 높인다. 높이를 안 주면 한 줄만 보이는 뷰어가 있다.
        # 스텝 수로만 세면 모자란다 — 한 스텝이 열 너비를 넘으면 두 줄이 되기 때문이다.
        # 실제로 모자라서 제출용 캡처에서 스텝이 세로로 잘렸다. 열 너비로 줄 수를 센다.
        # 여기 값은 넉넉한 추정이다. 다 돌린 뒤 엑셀에서 행 '자동 맞춤' 을 주면 정확한
        # 높이로 좁혀진다 — 모자라 잘리는 쪽보다 남는 쪽이 낫다.
        need = max(wrapped_lines(pre, 18.0),
                   wrapped_lines(cell.value, 26.0),
                   wrapped_lines(ws.cell(row, COL_EXPECT).value, 40.0))
        ws.row_dimensions[row].height = max(15, 13.5 * need)

        if write_pre and pre:
            ws.cell(row, COL_PRE).value = pre
        wrote += 1

    if unseen:
        print("  엑셀에 행이 없는 초안 항목:", ", ".join(sorted(unseen)))

    # 요약 시트 — 출처에 초안 문서를 더하고, 초안임을 한 줄로 밝힌다.
    summary = wb["요약"]
    summary["B9"] = ("docs/qa/test-cases.md · docs/qa/test-run-log.md · "
                     "docs/qa/test-plan.md · docs/qa/test-steps.md")
    summary["A11"] = ("테스트 스텝과 사전 조건은 2026-09-28 초안이다 — 2회전에서 수행하며 확정한다"
                      if note else None)
    # 2회전부터 결과 열이 둘이다. H 가 최신이고 I 가 1회전 이력이다 — H 를 그대로 둔 것은
    # 집계 수식 59곳과 조건부 서식이 전부 H 를 보기 때문이다. 앞에 끼우면 전부 어긋난다.
    summary["A12"] = ("[결과] 는 최신 차수다. 1차 결과는 [1차 결과] 열에 남겨 같은 줄에서 "
                      "수정 전후가 보이게 했다 — 집계와 그래프는 [결과] 를 센다")
    # [결과] 가 2회전이므로 실패 내역도 2회전 기준이다. 1회전 12건은 [1회전 결과] 열에 있다.
    # 숫자를 손으로 적지 않는다 — 전에 4 + 7 = 11 로 적어 두고 실제 12 와 어긋났다.
    summary["A36"] = ("실패 5건 = 미수정 결함 3건(#165 #166 #164) · 안 고치기로 판단한 1건(#151) · "
                      "이연 1건(#143). 1회전 12건은 [1회전 결과] 열에 있다. "
                      "상세는 [결과 보고서] 시트")

    wb.save(BOOK)
    print("스텝을 넣은 행 %d · 비워 둔 행 %d · 초안 누락 %d" % (wrote, skipped, missing))
    print("사전 조건도 덮어썼나:", write_pre)


if __name__ == "__main__":
    main(sys.argv[1:])
