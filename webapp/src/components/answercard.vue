<script setup lang="ts">
    defineProps<{
        answer: string[]
    }>();
</script>

<template>
    <div class="card" v-if="answer.length > 0">
      <div v-for="(item, index) in answer" :key="index" class="answer-item">
        {{ item }}
      </div>
    </div>
</template>

<style scoped>
.card {
  background: #1a1818;
  border-radius: 10px;
  border: 1px solid #333;
  padding: 1.5rem;
  margin-top: 2rem;
  color: #fff;
  justify-content: left;
}

.answer-item {
  display: list-item;
  list-style-type: circle;
  list-style-position: outside;
  text-align: left;
  overflow-wrap: break-word;
  margin-left: 1rem;
  margin-bottom: 0.5rem;
}
</style>