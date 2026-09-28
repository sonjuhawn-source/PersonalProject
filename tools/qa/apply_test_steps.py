# -*- coding: utf-8 -*-
"""docs/qa/test-steps-draft.md 의 테스트 스텝을 WeaponHero_TC.xlsx 로 옮긴다.

원본은 md 이고 엑셀은 파생물이다. 엑셀을 손으로 고치면 둘이 갈라지므로,
스텝이 바뀌면 md 를 고치고 이 스크립트를 다시 돌린다.

    uv run --python 3.12 --with openpyxl python tools/qa/apply_test_steps.py
    ... --pre        사전 조건 열도 같이 덮어쓴다 (기본은 스텝만)
    ... --no-note    요약 시트의 초안 표시를 넣지 않는다 (스텝 확정 후)

엑셀이 Excel 에서 열려 있으면 쓰기가 막힌다. 닫고 돌린다.
"""
import re
import sys
from pathlib import Path

import openpyxl

ROOT = Path(__file__).resolve().parents[2]
DRAFT = ROOT / "docs" / "qa" / "test-steps-draft.md"
BOOK = ROOT / "docs" / "qa" / "WeaponHero_TC.xlsx"

COL_DETAIL = 4   # D 소분류(확인 항목) — 앞에 TC ID 가 붙어 있다
COL_PRE = 5      # E 사전 조건
COL_STEPS = 6    # F 테스트 스텝

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
            common = clean(s.split("—", 1)[1]) if "—" in s else ""
            continue

        if s.startswith("#### "):
            flush()
            m = ID.search(s)
            tc, pre, steps, in_block = (m.group(0) if m else None), "", [], False
            continue

        if tc and s.startswith("사전 조건") and "—" in s:
            pre = clean(s.split("—", 1)[1])
            continue

        if s.startswith("```"):
            in_block = not in_block
            continue

        if in_block and tc and s:
            steps.append(s)

    flush()
    return out


def clean(s):
    """마크다운 강조와 표시 꼬리를 벗긴다."""
    s = s.replace("**", "").replace("`", "")
    return s.strip()


def main(argv):
    write_pre = "--pre" in argv
    note = "--no-note" not in argv

    steps_by_tc = parse_draft(DRAFT.read_text(encoding="utf-8"))
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

        if not steps:                       # N/A 항목은 비운다
            ws.cell(row, COL_STEPS).value = None
            ws.row_dimensions[row].height = None
            skipped += 1
            continue

        cell = ws.cell(row, COL_STEPS)
        cell.value = "\n".join(steps)
        # 줄 수만큼 행을 높인다. 높이를 안 주면 한 줄만 보이는 뷰어가 있다.
        ws.row_dimensions[row].height = max(15, 13.5 * len(steps))

        if write_pre and pre:
            ws.cell(row, COL_PRE).value = pre
        wrote += 1

    if unseen:
        print("  엑셀에 행이 없는 초안 항목:", ", ".join(sorted(unseen)))

    # 요약 시트 — 출처에 초안 문서를 더하고, 초안임을 한 줄로 밝힌다.
    summary = wb["요약"]
    summary["B9"] = ("docs/qa/test-cases.md · docs/qa/test-run-log.md · "
                     "docs/qa/test-plan.md · docs/qa/test-steps-draft.md")
    summary["A11"] = ("테스트 스텝은 2026-09-28 초안이다 — 2회전에서 수행하며 확정한다"
                      if note else None)

    wb.save(BOOK)
    print("스텝을 넣은 행 %d · 비워 둔 행 %d · 초안 누락 %d" % (wrote, skipped, missing))
    print("사전 조건도 덮어썼나:", write_pre)


if __name__ == "__main__":
    main(sys.argv[1:])
