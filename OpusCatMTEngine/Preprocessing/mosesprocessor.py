#!/usr/bin/env python3
# -*- coding: utf-8 -*-

from sacremoses import MosesTruecaser, MosesTokenizer, MosesDetokenizer
import fileinput, sys, argparse, io

def postprocess(target_lang):
	md = MosesDetokenizer(lang="fi")
	utf8_stdin = io.TextIOWrapper(sys.stdin.buffer, encoding='utf-8')

	for line in utf8_stdin:
		#the input is from Marian which outputs utf-8
		desegmented = line.replace("@@ ","")
		detokenized = md.detokenize(desegmented.split())
		sys.stderr.write("sentence processed\n")
		sys.stdout.buffer.write((detokenized + "\n").encode("utf-8"))
		sys.stdout.flush()

def preprocess(source_lang,tcmodel,escape):
	utf8_stdin = io.TextIOWrapper(sys.stdin.buffer, encoding='utf-8')
	mtok = MosesTokenizer(lang=source_lang)
	mtr = MosesTruecaser(tcmodel)
	sys.stderr.write("model loaded\n")
	for line in utf8_stdin:
		tokenized = mtok.tokenize(line,escape=escape)
		truecased = mtr.truecase(" ".join(tokenized))
		sys.stderr.write("sentence processed\n")
		sys.stdout.buffer.write((" ".join(truecased) + "\n").encode("utf-8"))
		sys.stdout.flush()
		
if __name__ == "__main__":
	parser = argparse.ArgumentParser(description='Process some integers.')
	parser.add_argument('--stage', dest='stage',type=str,
                    help='either preprocess or postprocess')
	parser.add_argument('--escape', action='store_true',
                    help='escape special characters')
	parser.add_argument('--sourcelang', dest='sourcelang',type=str,
                    help='two letter lang code')
	parser.add_argument('--targetlang', dest='targetlang',type=str,
					help='two letter lang code')
	parser.add_argument('--tcmodel', dest='tcmodel',type=str,
					help='path to truecase model')
	args = parser.parse_args()
	source_lang = args.sourcelang
	target_lang = args.targetlang
	if (args.stage == "preprocess"):
		preprocess(source_lang,args.tcmodel,args.escape)
	if (args.stage == "postprocess"):
		postprocess(target_lang)