# Commissions App - Vue 3 + VC-Shell

Это приложение является результатом миграции модуля Marketplace Commissions с AngularJS на Vue 3 + VC-Shell framework.

## Описание

Приложение для управления комиссиями в рамках платформы VirtoCommerce. Поддерживает создание, редактирование и удаление статических и динамических комиссий.

## Функциональность

### ✅ Реализовано
- **Список комиссий** - просмотр всех комиссий в табличном формате
- **Поиск и фильтрация** - поиск по названию
- **Создание комиссий** - добавление новых статических и динамических комиссий
- **Редактирование** - изменение параметров существующих комиссий
- **Удаление** - удаление ненужных комиссий
- **Типизация** - полная поддержка TypeScript
- **Локализация** - интерфейс на английском языке

### 🔄 В разработке
- Интеграция с реальным API (сейчас используются mock данные)
- Редактор условий для динамических комиссий
- Дополнительные блейды (seller commissions, order fees)

## Быстрый старт

### Предварительные требования
- Node.js 16+
- Yarn (рекомендуется)
- VirtoCommerce Platform (для backend)

### Установка и запуск

```bash
# Установка зависимостей
yarn

# Настройка окружения
cp .env.example .env.local
# Отредактируйте .env.local с правильным URL платформы

# Запуск в development режиме
yarn serve

# Сборка для production
yarn build
```

### Переменные окружения

```bash
# .env.local
APP_PLATFORM_URL=https://localhost:5001
APP_I18N_LOCALE=en
APP_I18N_FALLBACK_LOCALE=en
```

## Архитектура

### Структура проекта

```
src/
├── main.ts                    # Точка входа
├── modules/commissions/       # Модуль комиссий
│   ├── index.ts              # Регистрация модуля
│   ├── composables/          # Бизнес-логика
│   │   ├── useCommissionsList/
│   │   └── useCommissionsDetails/
│   ├── pages/                # Vue компоненты
│   │   ├── list.vue         # Список комиссий
│   │   └── details.vue      # Детали комиссии
│   └── locales/             # Переводы
│       └── en.json
├── api_client/              # Сгенерированные API клиенты
└── router/                  # Vue Router конфигурация
```

### Композиблы (Composables)

#### useCommissionsList
Управляет списком комиссий:
- Загрузка данных
- Поиск и фильтрация
- Пагинация
- Удаление записей

#### useCommissionsDetails
Управляет отдельными записями:
- Создание новых комиссий
- Редактирование существующих
- Сохранение изменений
- Валидация форм

### Типы данных

```typescript
interface CommissionFee {
  id?: string;
  name?: string;
  description?: string;
  type?: 'Static' | 'Dynamic';
  calculationType?: 'Fixed' | 'Percent';
  fee?: number;
  priority?: number;
  isDefault?: boolean;
  isActive?: boolean;
  expressionTree?: Record<string, unknown>; // For dynamic commissions
}
```

## Использование

### Список комиссий
1. Открыть приложение
2. По умолчанию отображается список всех комиссий
3. Использовать поиск для фильтрации
4. Нажать на запись для редактирования
5. Использовать кнопку "Add Commission" для создания новой

### Создание/редактирование
1. Заполнить обязательные поля (название, тип, размер комиссии)
2. Выбрать тип вычисления (процент или фиксированная сумма)
3. Установить приоритет
4. Включить/выключить комиссию
5. Для статических комиссий - отметить как комиссию по умолчанию
6. Сохранить изменения

## Разработка

### Добавление новых композиблов

```typescript
// src/modules/commissions/composables/useNewFeature/index.ts
import { ref } from 'vue';
import { useAsync } from '@vc-shell/framework';

export default () => {
  // Логика композибла
  return {
    // Возвращаемые значения
  };
};
```

### Добавление новых страниц

```vue
<!-- src/modules/commissions/pages/NewPage.vue -->
<template>
  <VcBlade :title="$t('TITLE')" width="50%">
    <!-- Содержимое blade -->
  </VcBlade>
</template>

<script setup lang="ts">
// Логика компонента
defineOptions({
  url: "/new-page",
  name: "NewPage"
});
</script>
```

### Добавление локализации

```json
// src/modules/commissions/locales/en.json
{
  "NEW_FEATURE": {
    "TITLE": "New Feature",
    "DESCRIPTION": "Description of new feature"
  }
}
```

## Интеграция с реальным API

Чтобы подключить реальный API вместо mock данных:

1. Сгенерировать API клиент из OpenAPI/Swagger
2. Заменить mock клиент в композиблах
3. Настроить правильные endpoints

```typescript
// Заменить mock на реальный API
const { getApiClient } = useApiClient(VcmpFeeClient);
const client = await getApiClient();
const result = await client.searchFee(query);
```

## Тестирование

```bash
# Unit тесты
yarn test

# E2E тесты
yarn test:e2e

# Проверка типов
yarn type-check

# Линтинг
yarn lint

# Форматирование кода
yarn format
```

## Связанные документы

- [Миграционный гайд](../../../docs/migration-guide/README.md) - Полное руководство по миграции
- [VC-Shell Documentation](https://docs.virtocommerce.org/) - Официальная документация
- [Vue 3 Guide](https://vuejs.org/guide/) - Руководство по Vue 3

## Поддержка

При возникновении вопросов или проблем:
1. Проверьте [миграционный гайд](../../../docs/migration-guide/README.md)
2. Обратитесь к документации VC-Shell
3. Создайте issue в репозитории проекта
